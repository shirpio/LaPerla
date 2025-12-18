using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PracticaProfesional2025.Data;
using PracticaProfesional2025.Models;
using OfficeOpenXml;

namespace PracticaProfesional2025.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment env;

        public ProductosController(ApplicationDbContext context , IWebHostEnvironment env)
        {
            _context = context;
            this.env = env;
        }


        
        [HttpPost]
        public async Task<IActionResult> ImportarProducto(IFormFile archivo)
        {
            // ❌ Sin archivo
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "No se seleccionó ningún archivo para importar.";
                return RedirectToAction(nameof(Index));
            }

            int importados = 0;
            int duplicados = 0;

            try
            {
                using var stream = archivo.OpenReadStream();
                using var package = new ExcelPackage(stream);

                var worksheet = package.Workbook.Worksheets[0];

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var nombre = worksheet.Cells[row, 1].Text.Trim();
                    var precioTexto = worksheet.Cells[row, 2].Text.Trim();
                    var tipoTexto = worksheet.Cells[row, 3].Text.Trim();

                    if (string.IsNullOrWhiteSpace(nombre))
                        continue;

                    if (!decimal.TryParse(precioTexto, out decimal precio))
                        continue;

                    if (!Enum.TryParse<TipoVenta>(tipoTexto, true, out var tipo))
                        continue;

                    // 🚫 Evitar duplicados por nombre
                    bool existe = await _context.Productos
                        .AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower());

                    if (existe)
                    {
                        duplicados++;
                        continue;
                    }

                    var producto = new Producto
                    {
                        Nombre = nombre,
                        Precio = precio,
                        Tipo = tipo
                    };

                    _context.Productos.Add(producto);
                    importados++;
                }

                if (importados > 0)
                    await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Importación finalizada. " +
                    $"✔ {importados} productos agregados" +
                    (duplicados > 0 ? $" | ⚠ {duplicados} duplicados omitidos" : "");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al procesar el archivo.";
                return RedirectToAction(nameof(Index));
            }
        }




        // GET: Productos
        public IActionResult Index(string search, int page = 1)
        {
            int pageSize = 10;

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;

            var productos = _context.Productos
                .Where(p => p.Activo) // 👈 SOLO ACTIVOS
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                productos = productos.Where(p => p.Nombre.Contains(search));
            }

            int totalItems = productos.Count();
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var productosPaginados = productos
                .OrderBy(p => p.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View(productosPaginados);
        }




        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            ViewBag.Tipos = Enum.GetValues(typeof(TipoVenta))
                                .Cast<TipoVenta>()
                                .Select(t => new SelectListItem
                                {
                                    Value = t.ToString(),
                                    Text = t.ToString()
                                })
                                .ToList();

            return View();
        }

        // POST: Productos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Precio,Tipo")] Producto producto)
        {
            // Validar duplicado
            bool existe = await _context.Productos
                .AnyAsync(p => p.Nombre.ToLower() == producto.Nombre.ToLower());

            if (existe)
            {
                ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Producto creado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Volver a cargar tipos
            ViewBag.Tipos = Enum.GetValues(typeof(TipoVenta))
                .Cast<TipoVenta>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = t.ToString()
                })
                .ToList();

            return View(producto);
        }


        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            ViewBag.Tipos = Enum.GetValues(typeof(TipoVenta))
                                .Cast<TipoVenta>()
                                .Select(t => new SelectListItem
                                {
                                    Value = t.ToString(),
                                    Text = t.ToString()
                                })
                                .ToList();

            return View(producto);
        }

        // POST: Productos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Precio,Tipo")] Producto producto)
        {
            if (id != producto.Id)
                return NotFound();

            // Validar duplicado (excluyendo el mismo producto)
            bool existe = await _context.Productos.AnyAsync(p =>
                p.Nombre.ToLower() == producto.Nombre.ToLower() &&
                p.Id != producto.Id);

            if (existe)
            {
                ModelState.AddModelError("Nombre", "Ya existe otro producto con ese nombre.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Producto actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Tipos = Enum.GetValues(typeof(TipoVenta))
                .Cast<TipoVenta>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = t.ToString()
                })
                .ToList();

            return View(producto);
        }


        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            producto.Activo = false; // 👈 DESACTIVAR
            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Desactivados()
        {
            var productos = _context.Productos
                .Where(p => !p.Activo)
                .ToList();

            return View(productos);
        }
        [HttpPost]
        public async Task<IActionResult> Restaurar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            producto.Activo = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto restaurado correctamente.";
            return RedirectToAction(nameof(Index)); // 👈 mejor
        }


        [HttpPost]
        public async Task<IActionResult> EliminarDefinitivo(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            TempData["Error"] = "Producto eliminado definitivamente.";
            return RedirectToAction(nameof(Desactivados));
        }



        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
