using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PracticaProfesional2025.Data;
using PracticaProfesional2025.Helpers;
using PracticaProfesional2025.Models;
using PracticaProfesional2025.ViewModels;
using QuestPDF.Fluent;
using PracticaProfesional2025.PDFs;


namespace PracticaProfesional2025.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================
        //           INDEX
        // ================================
        public async Task<IActionResult> Index()
        {
            var viewModel = new VentaViewModel
            {
                Detalles = new List<VentaViewModel.DetalleTemporal>()
            };

            // Recuperar temporales si existen
            if (TempData["Detalles"] != null)
            {
                viewModel.Detalles = JsonConvert.DeserializeObject<List<VentaViewModel.DetalleTemporal>>(
                    TempData["Detalles"].ToString()
                );
                TempData.Keep("Detalles");
            }

            // Cargar productos
            ViewBag.Productos = _context.Productos;
            ViewBag.Productos = _context.Productos
             .Select(p => new
             {
                 p.Id,
                 p.Nombre,
                 Tipo = p.Tipo.ToString(),   // <-- ENVÍA EL TEXTO "Kilogramo", "Unidad", etc.
                 p.Precio
             })

                .ToList();

            return View(viewModel);
        }

        // ================================
        //      AGREGAR PRODUCTO
        // ================================
        [HttpPost]
        public IActionResult AgregarProducto(int productoId, decimal Cantidad)
        {
            if (Cantidad <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor que 0.";
                return RedirectToAction("Index");
            }

            var producto = _context.Productos.FirstOrDefault(p => p.Id == productoId);
            if (producto == null)
            {
                TempData["Error"] = "El producto no existe.";
                return RedirectToAction("Index");
            }

            var detalles = TempData.Get<List<VentaViewModel.DetalleTemporal>>("Detalles")
                             ?? new List<VentaViewModel.DetalleTemporal>();

            decimal cantidadReal;
            decimal subtotal;

            if (producto.Tipo == TipoVenta.Kilogramo)
            {
                // El usuario ingresa el importe directamente (subtotal)
                subtotal = Cantidad;

                // Cantidad real en kilos = importe / precioPorKg
                cantidadReal = Math.Round(subtotal / producto.Precio, 3);
            }
            else
            {
                cantidadReal = Cantidad;
                subtotal = Cantidad * producto.Precio;
            }

            detalles.Add(new VentaViewModel.DetalleTemporal
            {
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                TipoVenta = producto.Tipo.ToString(),
                Cantidad = cantidadReal,     // ← AQUÍ guardamos la cantidad real
                Precio = producto.Precio,
                Subtotal = subtotal,
                Fecha = DateTime.Now
            });

            TempData.Put("Detalles", detalles);
            TempData.Keep("Detalles");

            TempData["Success"] = $"{producto.Nombre} agregado correctamente.";
            return RedirectToAction("Index");
        }
        public IActionResult EditarDetalle(int index)
        {
            var detalles = TempData.Get<List<VentaViewModel.DetalleTemporal>>("Detalles")
                             ?? new List<VentaViewModel.DetalleTemporal>();

            if (index < 0 || index >= detalles.Count)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction("Index");
            }

            var item = detalles[index];

            var viewModel = new VentaViewModel
            {
                Detalles = detalles,
                EditIndex = index,
                ProductoEditarId = item.ProductoId,
                CantidadEditar = item.Cantidad
            };

            ViewBag.Productos = _context.Productos
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    Tipo = p.Tipo.ToString(),
                    p.Precio
                })
                .ToList();

            TempData.Put("Detalles", detalles);

            return View("Index", viewModel);
        }
        [HttpPost]
        public IActionResult GuardarEdicion(int index, int productoId, decimal cantidad)
        {
            var detalles = TempData.Get<List<VentaViewModel.DetalleTemporal>>("Detalles")
                             ?? new List<VentaViewModel.DetalleTemporal>();

            if (index < 0 || index >= detalles.Count)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction("Index");
            }

            var producto = _context.Productos.FirstOrDefault(p => p.Id == productoId);

            if (producto == null)
            {
                TempData["Error"] = "Producto inválido.";
                return RedirectToAction("Index");
            }

            decimal cantidadReal;
            decimal subtotal;

            if (producto.Tipo == TipoVenta.Kilogramo)
            {
                subtotal = cantidad;
                cantidadReal = subtotal / producto.Precio;
            }
            else
            {
                cantidadReal = cantidad;
                subtotal = cantidad * producto.Precio;
            }

            detalles[index] = new VentaViewModel.DetalleTemporal
            {
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                TipoVenta = producto.Tipo.ToString(),
                Cantidad = cantidadReal,
                Precio = producto.Precio,
                Subtotal = subtotal,
                Fecha = detalles[index].Fecha
            };

            TempData.Put("Detalles", detalles);

            TempData["Success"] = "Producto editado correctamente.";

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult EliminarDetalle(int index)
        {
            var detalles = TempData.Get<List<VentaViewModel.DetalleTemporal>>("Detalles")
                             ?? new List<VentaViewModel.DetalleTemporal>();

            if (index >= 0 && index < detalles.Count)
            {
                detalles.RemoveAt(index);

                TempData["Success"] = "Producto eliminado correctamente.";
            }

            TempData.Put("Detalles", detalles);
            TempData.Keep("Detalles");

            return RedirectToAction("Index");
        }

        // ================================
        //        CONFIRMAR VENTA
        // ================================
        [HttpPost]
        public IActionResult ConfirmarVenta()
        {
            var detalles = TempData.Get<List<VentaViewModel.DetalleTemporal>>("Detalles")
                            ?? new List<VentaViewModel.DetalleTemporal>();

            if (!detalles.Any())
            {
                TempData["Error"] = "No hay productos agregados.";
                return RedirectToAction("Index");
            }

            var venta = new Venta
            {
                Fecha = DateTime.Now
            };

            _context.Ventas.Add(venta);
            _context.SaveChanges();

            foreach (var d in detalles)
            {
                _context.DetalleVentas.Add(new DetalleVenta
                {
                    VentaId = venta.Id,
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    Subtotal = d.Subtotal,
                    Fecha = d.Fecha
                });
            }

            _context.SaveChanges();

            TempData.Remove("Detalles");
            TempData["Success"] = "Venta registrada correctamente.";

            return RedirectToAction("Index");
        }

        // ================================
        //      REPORTE POR FECHA
        // ================================
        public IActionResult ReportePorFecha(DateTime? desde, DateTime? hasta, int page = 1)
        {
            int pageSize = 7; // días por página

            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;
            ViewBag.CurrentPage = page;

            if (!desde.HasValue || !hasta.HasValue)
            {
                return View(new List<ReporteDiaViewModel>());
            }

            var fechaDesde = DateTime.SpecifyKind(desde.Value.Date, DateTimeKind.Utc);
            var fechaHasta = DateTime.SpecifyKind(hasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var detalles = _context.DetalleVentas
                .Where(d => d.Fecha >= fechaDesde &&
                            d.Fecha <= fechaHasta);
            var resumenQuery = detalles
                .GroupBy(d => d.Fecha.Date)
                .Select(g => new ReporteDiaViewModel
                {
                    Fecha = g.Key,
                    TotalDia = g.Sum(x => x.Subtotal)
                })
                .OrderBy(r => r.Fecha);

            int totalItems = resumenQuery.Count();
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var todosDetalles = detalles
    .Include(d => d.Producto)
    .ToList();

            decimal totalGeneral = todosDetalles.Sum(x => x.Subtotal ?? 0);

            int cantidadVentas = todosDetalles.Count;

            var productoUnidadTop = todosDetalles
                .Where(x => x.Producto != null &&
                            x.Producto.Tipo == TipoVenta.Unidad)
                .GroupBy(x => x.Producto!.Nombre)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Cantidad = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.Cantidad)
                .FirstOrDefault();

            var productoKgTop = todosDetalles
                .Where(x => x.Producto != null &&
                            x.Producto.Tipo == TipoVenta.Kilogramo)
                .GroupBy(x => x.Producto!.Nombre)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Total = g.Sum(x => x.Subtotal ?? 0)
                })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();
            var resumen = resumenQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (resumen.Any())
            {
                resumen[0].TotalGeneral = totalGeneral;

                resumen[0].CantidadVentas = cantidadVentas;

                resumen[0].ProductoUnidadTop =
                    productoUnidadTop?.Nombre ?? "Sin datos";

                resumen[0].CantidadUnidadTop =
                    productoUnidadTop?.Cantidad ?? 0;

                resumen[0].ProductoKgTop =
                    productoKgTop?.Nombre ?? "Sin datos";

                resumen[0].TotalKgTop =
                    productoKgTop?.Total ?? 0;
            }
            var graficoProductos = todosDetalles
                .Where(x => x.Producto != null &&
                            x.Producto.Tipo == TipoVenta.Unidad)
                .GroupBy(x => x.Producto!.Nombre)
                .Select(g => new
                {
                    Producto = g.Key,
                    Cantidad = g.Sum(x => x.Cantidad)   
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();

            ViewBag.GraficoLabels = graficoProductos
                .Select(x => x.Producto)
                .ToList();

            ViewBag.GraficoData = graficoProductos
                .Select(x => x.Cantidad)
                .ToList();

            var graficoKg = todosDetalles
                .Where(x => x.Producto != null &&
                            x.Producto.Tipo == TipoVenta.Kilogramo)
                .GroupBy(x => x.Producto!.Nombre)
                .Select(g => new
                {
                    Producto = g.Key,
                    Total = g.Sum(x => x.Subtotal ?? 0)
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            ViewBag.GraficoKgLabels = graficoKg
                .Select(x => x.Producto)
                .ToList();

            ViewBag.GraficoKgData = graficoKg
                .Select(x => x.Total)
                .ToList();
            var ventasPorDia = todosDetalles
                .GroupBy(x => x.Fecha.Date)
                .Select(g => new
                {
                    Fecha = g.Key.ToString("dd/MM"),
                    Total = g.Sum(x => x.Subtotal ?? 0)
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            ViewBag.VentasDiaLabels = ventasPorDia
                .Select(x => x.Fecha)
                .ToList();

            ViewBag.VentasDiaData = ventasPorDia
                .Select(x => x.Total)
                .ToList();
            return View(resumen);
        }


        public IActionResult DetallePorDia(DateTime fecha, DateTime? desde, DateTime? hasta)
        {
            var detalles = _context.DetalleVentas
                .Include(d => d.Producto)
                .Where(d => d.Fecha.Date == fecha.Date)
                .OrderBy(d => d.Fecha)
                .ToList();

            ViewBag.Fecha = fecha;
            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;

            return View(detalles);
        }
        public IActionResult ExportarDetalleDiaPdf(DateTime fecha)
        {
            var detalles = _context.DetalleVentas
                .Include(d => d.Producto)
                .Where(d => d.Fecha.Date == fecha.Date)
                .OrderBy(d => d.Fecha)
                .ToList();

            var documento = new ReporteDiaPdf(detalles, fecha);

            var pdf = documento.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Reporte_{fecha:dd-MM-yyyy}.pdf"
            );
        }



    }
}

