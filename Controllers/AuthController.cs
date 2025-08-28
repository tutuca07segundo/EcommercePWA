using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ApplicationDbContext context, IEmailService emailService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;
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

        // GET: /Auth/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Auth/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Informe seu e-mail";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
            if (user == null)
            {
                TempData["SuccessMessage"] = "Se existir, enviaremos um e-mail com o código.";
                return View();
            }

            var code = new Random().Next(100000, 999999).ToString();
            var request = new PasswordResetRequest
            {
                Email = user.Email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Used = false
            };
            _context.PasswordResetRequests.Add(request);
            await _context.SaveChangesAsync();

            var link = Url.Action("ResetPassword", "Auth", new { email = user.Email }, Request.Scheme);
            var body = $"Seu código de recuperação: {code}\n\nUse-o em até 15 minutos em: {link}";
            var sent = await _emailService.SendSimpleAsync(user.Email, "Recuperação de senha", body);
            if (!sent)
            {
                TempData["ErrorMessage"] = "Falha ao enviar e-mail. Verifique as configurações SMTP.";
                return View();
            }

            TempData["SuccessMessage"] = "Se existir, enviaremos um e-mail com o código.";
            return RedirectToAction("ForgotPassword");
        }

        // GET: /Auth/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // POST: /Auth/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string code, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["ErrorMessage"] = "Preencha todos os campos.";
                ViewBag.Email = email;
                return View();
            }

            var req = await _context.PasswordResetRequests
                .Where(r => r.Email.ToLower() == email.ToLower() && r.Code == code && !r.Used && r.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(r => r.Id)
                .FirstOrDefaultAsync();

            if (req == null)
            {
                TempData["ErrorMessage"] = "Código inválido ou expirado.";
                ViewBag.Email = email;
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuário não encontrado.";
                return View();
            }

            user.Password = _authService.HashPassword(newPassword);
            req.Used = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Senha alterada com sucesso. Faça login.";
            return RedirectToAction("Login");
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
            return View(new RegisterViewModel());
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var existingUser = await _authService.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Este email já está em uso");
                    return View(model);
                }
                var user = await _authService.RegisterAsync(model.Name, model.Email, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Erro ao criar conta. Tente novamente.");
                    return View(model);
                }
                TempData["SuccessMessage"] = "Conta criada com sucesso! Faça login para continuar.";
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o registro");
                ModelState.AddModelError("", "Erro durante o registro. Tente novamente.");
                return View(model);
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
