using Microsoft.AspNetCore.Mvc;
using TallerElectronika.Data;
using TallerElectronika.Models;
using System.Security.Cryptography;
using System.Text;

namespace TallerElectronika.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Login
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string nombreUsuario, string password)
        {
            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Usuario y contraseña son requeridos";
                return View();
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            if (!VerificarPassword(password, usuario.PasswordHash))
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            if (!usuario.Activo)
            {
                ViewBag.Error = "Usuario inactivo";
                return View();
            }

            // Guardar datos en sesión
            HttpContext.Session.SetString("UsuarioId", usuario.UsuarioId.ToString());
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
            HttpContext.Session.SetString("Rol", usuario.Rol);

            return RedirectToAction("Dashboard", "Home");
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        // Métodos privados
        private bool VerificarPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}