using System.Security.Cryptography;
using System.Text;
using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public class AuthService : IAuthService
    {
        private readonly List<User> _users;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;
            _users = new List<User>();
            
            // Criar usuário admin padrão
            _ = CreateAdminUserAsync();
        }

        public Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
            if (user == null) return Task.FromResult<User?>(null);

            if (VerifyPassword(password, user.Password))
            {
                return Task.FromResult<User?>(user);
            }

            return Task.FromResult<User?>(null);
        }

        public Task<User?> RegisterAsync(string name, string email, string password)
        {
            if (_users.Any(u => u.Email.ToLower() == email.ToLower()))
            {
                return Task.FromResult<User?>(null); // Email já existe
            }

            var user = new User
            {
                Id = _users.Count + 1,
                Name = name,
                Email = email,
                Password = HashPassword(password),
                Role = "User",
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _users.Add(user);
            return Task.FromResult<User?>(user);
        }

        public Task<User?> GetUserByIdAsync(int id)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Id == id && u.IsActive));
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && u.IsActive));
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public Task<bool> CreateAdminUserAsync()
        {
            if (_users.Any(u => u.Role == "Admin"))
            {
                return Task.FromResult(true); // Admin já existe
            }

            var adminUser = new User
            {
                Id = 1,
                Name = "Administrador",
                Email = "admin@kaboom.com",
                Password = HashPassword("admin123"),
                Role = "Admin",
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _users.Add(adminUser);
            _logger.LogInformation("Usuário admin criado: admin@kaboom.com / admin123");
            return Task.FromResult(true);
        }
    }
}
