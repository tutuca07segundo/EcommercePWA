using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Services;
using EcommercePWA.Models;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    [RequireAdmin]
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(IAuthService authService, ILogger<UserController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            user.Password = string.Empty; // não exibir hash
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password")] User user)
        {
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

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ok = await _authService.SoftDeleteUserAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Usuário excluído com sucesso" : "Erro ao excluir usuário";
            return RedirectToAction("Users", "Admin");
        }

        public async Task<IActionResult> ToggleAdmin(int id)
        {
            var ok = await _authService.PromoteToAdminAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Usuário promovido a administrador" : "Erro ao promover usuário";
            return RedirectToAction("Users", "Admin");
        }
    }
}


