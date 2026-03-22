using Microsoft.AspNetCore.Mvc;
using CeplanAuth.Data;
using CeplanAuth.Models;

namespace CeplanAuth.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUsuarioRepository _repo;

        public HomeController(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        // ── Guard: verificar sesión activa ────────────────────────
        private bool SesionActiva()
        {
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (!uid.HasValue) return false;

            var ultimaStr = HttpContext.Session.GetString("UltimaActividad");
            if (string.IsNullOrEmpty(ultimaStr)) return false;

            var ultima = DateTime.Parse(ultimaStr);
            if ((DateTime.Now - ultima).TotalMinutes >= 20)
            {
                HttpContext.Session.Clear();
                return false;
            }

            // Actualizar actividad en cada request
            HttpContext.Session.SetString("UltimaActividad", DateTime.Now.ToString("o"));
            return true;
        }

        // ── GET: /Home/Perfil ─────────────────────────────────────
        public async Task<IActionResult> Perfil()
        {
            if (!SesionActiva())
                return RedirectToAction("SesionExpirada", "Account");

            var uid = HttpContext.Session.GetInt32("UsuarioId")!.Value;
            var usuario = await _repo.ObtenerUsuarioPorIdAsync(uid);

            if (usuario == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            return View(usuario);
        }

        // ── GET: /Home/Error ──────────────────────────────────────
        public IActionResult Error()
        {
            return View();
        }
    }
}
