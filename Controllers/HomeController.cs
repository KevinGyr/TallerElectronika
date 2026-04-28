using Microsoft.AspNetCore.Mvc;
using TallerElectronika.Data;

namespace TallerElectronika.Controllers
{
    [Auth]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            try
            {
                var totalClientes = _context.Clientes.Where(c => c.Activo).Count();
                var totalProductos = _context.Productos.Where(p => p.Activo).Count();
                var totalVentas = _context.Ventas.Count();
                var ventasTotales = _context.Ventas.Sum(v => (decimal?)v.Total) ?? 0;

                ViewBag.TotalClientes = totalClientes;
                ViewBag.TotalProductos = totalProductos;
                ViewBag.TotalVentas = totalVentas;
                ViewBag.VentasTotales = ventasTotales;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Dashboard: {ex.Message}");
                return View();
            }
        }
    }
}