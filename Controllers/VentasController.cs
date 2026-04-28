using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerElectronika.Data;
using TallerElectronika.Models;
using System.Collections.Generic;
using System.Linq;

namespace TallerElectronika.Controllers
{
    [Auth]
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index
        public IActionResult Index()
        {
            try
            {
                var ventas = _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Producto)
                    .OrderByDescending(v => v.FechaVenta)
                    .ToList();

                return View(ventas);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en Index: " + ex.Message);
                return View(new List<Venta>());
            }
        }

        // GET: Create
        public IActionResult Create()
        {
            try
            {
                ViewBag.Clientes = _context.Clientes.Where(c => c.Activo).ToList();
                ViewBag.Productos = _context.Productos.Where(p => p.Activo).ToList();
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en Create GET: " + ex.Message);
                return View();
            }
        }

        // POST: Create
        [HttpPost]
        public IActionResult Create(Venta venta)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine("Error de validación: " + error.ErrorMessage);
                }
                ViewBag.Clientes = _context.Clientes.Where(c => c.Activo).ToList();
                ViewBag.Productos = _context.Productos.Where(p => p.Activo).ToList();
                return View(venta);
            }

            try
            {
                venta.FechaVenta = DateTime.Now;
                venta.Estado = "Completada";
                venta.Impuesto = venta.SubTotal * 0.16m;
                venta.Total = venta.SubTotal + venta.Impuesto;

                _context.Ventas.Add(venta);
                _context.SaveChanges();

                Console.WriteLine("Venta #" + venta.VentaId + " creada. Procesando detalles...");

                // Obtener los productos agregados del formulario
                var productosAgregados = ObtenerProductosDelFormulario();

                Console.WriteLine("Se encontraron " + productosAgregados.Count + " productos agregados");

                foreach (var producto in productosAgregados)
                {
                    try
                    {
                        // Crear VentaDetalle
                        var ventaDetalle = new VentaDetalle
                        {
                            VentaId = venta.VentaId,
                            ProductoId = producto["productoId"],
                            Cantidad = producto["cantidad"],
                            PrecioUnitario = producto["precio"],
                            Subtotal = producto["subtotal"]
                        };

                        _context.VentaDetalles.Add(ventaDetalle);
                        Console.WriteLine("VentaDetalle creado: Producto " + producto["productoId"] + ", Cantidad " + producto["cantidad"]);

                        // Actualizar stock del producto
                        var productoActual = _context.Productos.Find(producto["productoId"]);
                        if (productoActual != null)
                        {
                            var stockAnterior = productoActual.Stock;
                            productoActual.Stock -= producto["cantidad"];

                            Console.WriteLine("Stock actualizado para " + productoActual.Nombre + ":");
                            Console.WriteLine("  - Stock anterior: " + stockAnterior);
                            Console.WriteLine("  - Stock nuevo: " + productoActual.Stock);

                            // Registrar movimiento de inventario
                            var movimiento = new MovimientoInventario
                            {
                                ProductoId = producto["productoId"],
                                TipoMovimiento = "Salida",
                                Cantidad = producto["cantidad"],
                                Motivo = "Venta #" + venta.VentaId,
                                FechaMovimiento = DateTime.Now,
                                UsuarioRegistro = HttpContext.Session.GetString("NombreUsuario")
                            };

                            _context.MovimientosInventario.Add(movimiento);
                            Console.WriteLine("Movimiento de inventario registrado para Venta #" + venta.VentaId);
                        }
                        else
                        {
                            Console.WriteLine("ADVERTENCIA: Producto " + producto["productoId"] + " no encontrado");
                        }
                    }
                    catch (Exception detalleEx)
                    {
                        Console.WriteLine("Error procesando detalle: " + detalleEx.Message);
                        throw;
                    }
                }

                _context.SaveChanges();

                Console.WriteLine("✅ Venta #" + venta.VentaId + " guardada exitosamente con " + productosAgregados.Count + " detalles");
                Console.WriteLine("Subtotal: " + venta.SubTotal + ", IVA: " + venta.Impuesto + ", Total: " + venta.Total);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al guardar venta: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                ViewBag.Clientes = _context.Clientes.Where(c => c.Activo).ToList();
                ViewBag.Productos = _context.Productos.Where(p => p.Activo).ToList();
                return View(venta);
            }
        }

        // GET: Details
        public IActionResult Details(int id)
        {
            try
            {
                var venta = _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.Producto)
                    .FirstOrDefault(v => v.VentaId == id);

                if (venta == null)
                {
                    Console.WriteLine("Venta " + id + " no encontrada");
                    return NotFound();
                }

                return View(venta);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en Details: " + ex.Message);
                return NotFound();
            }
        }

        // Método auxiliar para obtener productos del formulario
        private List<Dictionary<string, dynamic>> ObtenerProductosDelFormulario()
        {
            var productos = new List<Dictionary<string, dynamic>>();
            var form = Request.Form;
            var contador = 0;

            Console.WriteLine("Intentando obtener productos del formulario...");

            while (form.ContainsKey("productosAgregados[" + contador + "].productoId"))
            {
                try
                {
                    var productoIdStr = form["productosAgregados[" + contador + "].productoId"];
                    var cantidadStr = form["productosAgregados[" + contador + "].cantidad"];
                    var precioStr = form["productosAgregados[" + contador + "].precio"];
                    var subtotalStr = form["productosAgregados[" + contador + "].subtotal"];

                    Console.WriteLine("Producto " + contador + ": ID=" + productoIdStr + ", Cantidad=" + cantidadStr + ", Precio=" + precioStr + ", Subtotal=" + subtotalStr);

                    var productoId = int.Parse(productoIdStr);
                    var cantidad = int.Parse(cantidadStr);
                    var precio = decimal.Parse(precioStr);
                    var subtotal = decimal.Parse(subtotalStr);

                    productos.Add(new Dictionary<string, dynamic>
                    {
                        { "productoId", productoId },
                        { "cantidad", cantidad },
                        { "precio", precio },
                        { "subtotal", subtotal }
                    });

                    contador++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al procesar producto " + contador + ": " + ex.Message);
                    contador++;
                }
            }

            Console.WriteLine("Total de productos obtenidos: " + productos.Count);
            return productos;
        }
    }
}