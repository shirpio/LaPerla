using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PracticaProfesional2025.Data;
using PracticaProfesional2025.Helpers;
using PracticaProfesional2025.Models;
using PracticaProfesional2025.ViewModels;
using Rotativa.AspNetCore;


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
                cantidadReal = subtotal / producto.Precio;
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

            var detalles = _context.DetalleVentas
                .Where(d => d.Fecha.Date >= desde.Value.Date &&
                            d.Fecha.Date <= hasta.Value.Date);

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

            var resumen = resumenQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            return new ViewAsPdf("DetallePorDiaPdf", detalles)
            {
                FileName = $"Reporte_{fecha:dd-MM-yyyy}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }



    }
}

