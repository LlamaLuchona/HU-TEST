using Microsoft.AspNetCore.Mvc;
using CeplanAuth.Models;
using CeplanAuth.Data;

namespace CeplanAuth.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUsuarioRepository _repo;
        private readonly IHttpContextAccessor _httpContext;

        public AccountController(IUsuarioRepository repo, IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _httpContext = httpContext;
        }

        // ── GET: /Account/Login ───────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? tipoDoc)
        {
            // Si ya tiene sesión, ir al perfil
            if (HttpContext.Session.GetInt32("UsuarioId").HasValue)
                return RedirectToAction("Perfil", "Home");

            var vm = new LoginViewModel
            {
                TipoDocumento = tipoDoc ?? "DNI"
            };
            return View(vm);
        }

        // ── POST: /Account/Login ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ip = _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

            var resultado = await _repo.ValidarLoginAsync(
                model.TipoDocumento,
                model.Usuario.Trim(),
                model.Contrasena,
                ip);

            switch (resultado.Resultado)
            {
                case "OK":
                    // Guardar sesión
                    HttpContext.Session.SetInt32("UsuarioId", resultado.UsuarioId!.Value);
                    HttpContext.Session.SetString("TipoDocumento", model.TipoDocumento);
                    HttpContext.Session.SetString("NumeroDocumento", model.Usuario.Trim());
                    HttpContext.Session.SetString("UltimaActividad", DateTime.Now.ToString("o"));
                    return RedirectToAction("Perfil", "Home");

                case "CUENTA_BLOQUEADA":
                    return RedirectToAction("CuentaBloqueada");

                default: // CREDENCIALES_INVALIDAS
                    model.MensajeError = resultado.Mensaje;
                    model.TipoError = resultado.Resultado;
                    model.CVF = resultado.CVF;
                    return View(model);
            }
        }

        // ── GET: /Account/CuentaBloqueada ─────────────────────────
        [HttpGet]
        public IActionResult CuentaBloqueada()
        {
            return View();
        }

        // ── POST: /Account/Logout ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ── GET: /Account/SesionExpirada ──────────────────────────
        [HttpGet]
        public IActionResult SesionExpirada()
        {
            HttpContext.Session.Clear();
            return View();
        }

        // ── POST: /Account/ExtenderSesion (AJAX) ──────────────────
        [HttpPost]
        public IActionResult ExtenderSesion()
        {
            if (!HttpContext.Session.GetInt32("UsuarioId").HasValue)
                return Json(new { success = false });

            HttpContext.Session.SetString("UltimaActividad", DateTime.Now.ToString("o"));
            return Json(new { success = true });
        }

        // ── GET: /Account/VerificarSesion (AJAX polling) ──────────
        [HttpGet]
        public IActionResult VerificarSesion()
        {
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (!uid.HasValue)
                return Json(new { valida = false, minutosRestantes = 0 });

            var ultimaStr = HttpContext.Session.GetString("UltimaActividad");
            if (string.IsNullOrEmpty(ultimaStr))
                return Json(new { valida = false, minutosRestantes = 0 });

            var ultima = DateTime.Parse(ultimaStr);
            var minutosPasados = (DateTime.Now - ultima).TotalMinutes;
            var minutosRestantes = Math.Max(0, 20 - minutosPasados);

            return Json(new { valida = minutosPasados < 20, minutosRestantes = (int)minutosRestantes });
        }
    }
}
