using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Services;
using EcommercePWA.Models;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(IAuthService authService, ILogger<UserController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [RequireAuth]
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [RequireAuth]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            user.Password = string.Empty; // não exibir hash
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password")] User user)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            if (id != user.Id) return NotFound();

            var updated = await _authService.UpdateUserAsync(user);
            if (updated == null)
            {
                ModelState.AddModelError(string.Empty, "Erro ao atualizar usuário");
                return View(user);
            }
            TempData["SuccessMessage"] = "Usuário atualizado com sucesso";
            return RedirectToAction("Users", "Admin");
        }

        [RequireAuth]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var ok = await _authService.SoftDeleteUserAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Usuário excluído com sucesso" : "Erro ao excluir usuário";
            return RedirectToAction("Users", "Admin");
        }

        [RequireAuth]
        public async Task<IActionResult> ToggleAdmin(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var ok = await _authService.PromoteToAdminAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Usuário promovido a administrador" : "Erro ao promover usuário";
            return RedirectToAction("Users", "Admin");
        }
    }
}


