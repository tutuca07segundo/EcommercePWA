using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{
    /// <summary>
    /// API para autenticação e gerenciamento de usuários
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
        /// Realiza login do usuário
        /// </summary>
        /// <param name="request">Credenciais de login</param>
        /// <returns>Dados do usuário autenticado</returns>
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
        /// <param name="request">Dados do novo usuário</param>
        /// <returns>Status do registro</returns>
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
    /// Modelo para requisição de login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>Email do usuário</summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>Senha do usuário</summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para resposta de login
    /// </summary>
    public class LoginResponse
    {
        /// <summary>ID do usuário</summary>
        public int UserId { get; set; }
        
        /// <summary>Nome do usuário</summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>Email do usuário</summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>Role do usuário (User ou Admin)</summary>
        public string Role { get; set; } = string.Empty;
        
        /// <summary>Status do login</summary>
        public bool Success { get; set; }
    }

    /// <summary>
    /// Modelo para requisição de registro
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>Nome do usuário</summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>Email do usuário</summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>Senha do usuário</summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para resposta de registro
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>Status do registro</summary>
        public bool Success { get; set; }
        
        /// <summary>Mensagem de retorno</summary>
        public string Message { get; set; } = string.Empty;
    }
}
