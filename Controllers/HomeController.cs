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


            HttpContext.Session.SetString("UltimaActividad", DateTime.Now.ToString("o"));
            return true;
        }

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

        public IActionResult Error()
        {
            return View();
        }
    }
}
