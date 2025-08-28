using System.Security.Cryptography;
using System.Text;
using EcommercePWA.Models;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Users.Where(u => u.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os usuários");
                return new List<User>();
            }
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
                
                if (user == null) return null;

                if (VerifyPassword(password, user.Password))
                {
                    return user;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante autenticação do usuário {Email}", email);
                return null;
            }
        }

        public async Task<User?> RegisterAsync(string name, string email, string password)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
                {
                    return null; // Email já existe
                }

                var user = new User
                {
                    Name = name,
                    Email = email,
                    Password = HashPassword(password),
                    Role = "User",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Novo usuário registrado: {Email}", email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante registro do usuário {Email}", email);
                return null;
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por ID {Id}", id);
                return null;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por email {Email}", email);
                return null;
            }
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

        public async Task<bool> CreateAdminUserAsync()
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Role == "Admin"))
                {
                    return true; // Admin já existe
                }

                var adminUser = new User
                {
                    Name = "Administrador",
                    Email = "admin@kaboom.com",
                    Password = HashPassword("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Usuário admin criado: admin@kaboom.com / admin123");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário admin");
                return false;
            }
        }

        public async Task<bool> InitializeDatabaseAsync()
        {
            try
            {
                // Garantir que o banco existe
                await _context.Database.EnsureCreatedAsync();
                
                // Criar usuário admin se não existir
                await CreateAdminUserAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar banco de dados");
                return false;
            }
        }
    }
}
