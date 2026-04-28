using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerElectronika.Data;
using TallerElectronika.Models;

namespace TallerElectronika.Controllers
{
    [Auth]
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index
        public IActionResult Index()
        {
            try
            {
                var productos = _context.Productos.Where(p => p.Activo).ToList();
                return View(productos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Index: {ex.Message}");
                return View(new List<Producto>());
            }
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        public IActionResult Create(Producto producto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Error de validación: {error.ErrorMessage}");
                }
                return View(producto);
            }

            try
            {
                producto.Activo = true;
                producto.FechaCreacion = DateTime.Now;

                _context.Productos.Add(producto);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar producto: {ex.Message}");
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                return View(producto);
            }
        }

        // GET: Edit
        public IActionResult Edit(int id)
        {
            try
            {
                var producto = _context.Productos.Find(id);
                if (producto == null)
                    return NotFound();

                return View(producto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Edit GET: {ex.Message}");
                return NotFound();
            }
        }

        // POST: Edit
        [HttpPost]
        public IActionResult Edit(int id, Producto producto)
        {
            if (id != producto.ProductoId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Error de validación: {error.ErrorMessage}");
                }
                return View(producto);
            }

            try
            {
                var productoExistente = _context.Productos.Find(id);
                if (productoExistente == null)
                    return NotFound();

                productoExistente.Nombre = producto.Nombre;
                productoExistente.CodigoProducto = producto.CodigoProducto;
                productoExistente.Descripcion = producto.Descripcion;
                productoExistente.Categoria = producto.Categoria;
                productoExistente.PrecioUnitario = producto.PrecioUnitario;
                productoExistente.Stock = producto.Stock;
                productoExistente.StockMinimo = producto.StockMinimo;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar producto: {ex.Message}");
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                return View(producto);
            }
        }

        // POST: Delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var producto = _context.Productos.Find(id);
                if (producto == null)
                    return NotFound();

                producto.Activo = false;
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar producto: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // GET: Details
        public IActionResult Details(int id)
        {
            try
            {
                var producto = _context.Productos.Find(id);
                if (producto == null)
                    return NotFound();

                return View(producto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Details: {ex.Message}");
                return NotFound();
            }
        }

        // GET: Movimientos
        public IActionResult Movimientos(int id)
        {
            try
            {
                var movimientos = _context.MovimientosInventario
                    .Where(m => m.ProductoId == id)
                    .OrderByDescending(m => m.FechaMovimiento)
                    .ToList();

                return View(movimientos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Movimientos: {ex.Message}");
                return View(new List<MovimientoInventario>());
            }
        }
    }
}