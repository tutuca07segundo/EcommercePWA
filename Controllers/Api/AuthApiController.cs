using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{
    /// <summary>
    /// API de Autenticação - Login e Registro de Usuários
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthApiController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Realiza login de usuário
        /// </summary>
        /// <param name="request">Credenciais de login (email e senha)</param>
        /// <returns>Dados do usuário autenticado</returns>
        /// <response code="200">Login realizado com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="401">Credenciais incorretas</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _authService.AuthenticateAsync(request.Email, request.Password);
                if (user == null)
                    return Unauthorized(new { error = "Credenciais inválidas" });

                return Ok(new LoginResponse 
                { 
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        /// <param name="request">Dados do novo usuário (nome, email, senha)</param>
        /// <returns>Confirmação de registro</returns>
        /// <response code="200">Usuário registrado com sucesso</response>
        /// <response code="400">Erro ao registrar (email já existe ou dados inválidos)</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.RegisterAsync(request.Name, request.Email, request.Password);
                if (user == null)
                    return BadRequest(new { error = "Erro ao registrar usuário" });

                return Ok(new RegisterResponse 
                { 
                    Success = true,
                    Message = "Usuário registrado com sucesso"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo de requisição de login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        /// <example>admin@kaboom.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário
        /// </summary>
        /// <example>admin123</example>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta de login bem-sucedido
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// ID do usuário
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Nome do usuário
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Papel do usuário (admin ou user)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o login foi bem-sucedido
        /// </summary>
        public bool Success { get; set; }
    }

    /// <summary>
    /// Modelo de requisição de registro
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        /// <example>João Silva</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        /// <example>joao@email.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha (mínimo 6 caracteres)
        /// </summary>
        /// <example>senha123</example>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta de registro
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>
        /// Indica se o registro foi bem-sucedido
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem de confirmação ou erro
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
