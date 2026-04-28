using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerElectronika.Data;
using TallerElectronika.Models;

namespace TallerElectronika.Controllers
{
    [Auth]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index
        public IActionResult Index()
        {
            try
            {
                var clientes = _context.Clientes.Where(c => c.Activo).ToList();
                return View(clientes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Index: {ex.Message}");
                return View(new List<Cliente>());
            }
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        public IActionResult Create(Cliente cliente)
        {
            // Validar modelo
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Error de validación: {error.ErrorMessage}");
                }
                return View(cliente);
            }

            try
            {
                cliente.Activo = true;
                cliente.FechaRegistro = DateTime.Now;

                _context.Clientes.Add(cliente);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar cliente: {ex.Message}");
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                return View(cliente);
            }
        }

        // GET: Edit
        public IActionResult Edit(int id)
        {
            try
            {
                var cliente = _context.Clientes.Find(id);
                if (cliente == null)
                    return NotFound();

                return View(cliente);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Edit GET: {ex.Message}");
                return NotFound();
            }
        }

        // POST: Edit
        [HttpPost]
        public IActionResult Edit(int id, Cliente cliente)
        {
            if (id != cliente.ClienteId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Error de validación: {error.ErrorMessage}");
                }
                return View(cliente);
            }

            try
            {
                var clienteExistente = _context.Clientes.Find(id);
                if (clienteExistente == null)
                    return NotFound();

                clienteExistente.Nombre = cliente.Nombre;
                clienteExistente.Apellido = cliente.Apellido;
                clienteExistente.Email = cliente.Email;
                clienteExistente.Telefono = cliente.Telefono;
                clienteExistente.Direccion = cliente.Direccion;
                clienteExistente.Ciudad = cliente.Ciudad;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar cliente: {ex.Message}");
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                return View(cliente);
            }
        }

        // POST: Delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var cliente = _context.Clientes.Find(id);
                if (cliente == null)
                    return NotFound();

                cliente.Activo = false;
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar cliente: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // GET: Details
        public IActionResult Details(int id)
        {
            try
            {
                var cliente = _context.Clientes.Include(c => c.Ventas).FirstOrDefault(c => c.ClienteId == id);
                if (cliente == null)
                    return NotFound();

                return View(cliente);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Details: {ex.Message}");
                return NotFound();
            }
        }
    }
}