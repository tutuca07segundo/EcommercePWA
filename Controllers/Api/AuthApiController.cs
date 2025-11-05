using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{

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

    public class LoginRequest
    {

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {

        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool Success { get; set; }
    }

    public class RegisterRequest
    {

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {

        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
