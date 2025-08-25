using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: /Auth/Login
        public IActionResult Login(string? returnUrl = null, int? mustLoginToBuy = null)
        {
            if (mustLoginToBuy == 1)
            {
                ViewData["MustLoginToBuyMessage"] = "Acesse sua conta para continuar a compra.";
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Email e senha são obrigatórios");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                var user = await _authService.AuthenticateAsync(email, password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email ou senha inválidos");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                // Criar sessão do usuário
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);

                _logger.LogInformation($"Usuário {user.Email} fez login com sucesso");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o login");
                ModelState.AddModelError("", "Erro durante o login. Tente novamente.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }
        }

        // GET: /Auth/Register
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword, string? returnUrl = null)
        {
            try
            {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Todos os campos são obrigatórios");
                    return View();
                }

                if (password != confirmPassword)
                {
                    ModelState.AddModelError("", "As senhas não coincidem");
                    return View();
                }

                if (password.Length < 6)
                {
                    ModelState.AddModelError("", "A senha deve ter pelo menos 6 caracteres");
                    return View();
                }

                var existingUser = await _authService.GetUserByEmailAsync(email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Este email já está em uso");
                    return View();
                }

                var user = await _authService.RegisterAsync(name, email, password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Erro ao criar conta. Tente novamente.");
                    return View();
                }

                _logger.LogInformation($"Novo usuário registrado: {user.Email}");

                TempData["SuccessMessage"] = "Conta criada com sucesso! Faça login para continuar.";
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o registro");
                ModelState.AddModelError("", "Erro durante o registro. Tente novamente.");
                return View();
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
