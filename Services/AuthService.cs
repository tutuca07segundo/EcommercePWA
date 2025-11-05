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
                _logger.LogInformation("RegisterAsync iniciado para: {Email}", email);
                
                // Verificar se email já existe E está ativo
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive))
                {
                    _logger.LogWarning("Email já existe no banco (ativo): {Email}", email);
                    return null; // Email já existe
                }
                
                // Se email existe mas está inativo, reativar o usuário
                var inactiveUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsActive);
                if (inactiveUser != null)
                {
                    _logger.LogInformation("Reativando usuário inativo: {Email}", email);
                    inactiveUser.Name = name;
                    inactiveUser.Password = HashPassword(password);
                    inactiveUser.Role = "User"; // Sempre resetar para usuário comum
                    inactiveUser.IsActive = true;
                    inactiveUser.CreatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Usuário reativado com sucesso como User: {Email}", email);
                    return inactiveUser;
                }

                var hashedPassword = HashPassword(password);
                _logger.LogInformation("Senha hasheada. Tamanho: {Length} caracteres", hashedPassword.Length);

                var user = new User
                {
                    Name = name,
                    Email = email,
                    Password = hashedPassword,
                    Role = "User",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _logger.LogInformation("Adicionando usuário ao contexto: {Email}", email);
                _context.Users.Add(user);
                
                _logger.LogInformation("Salvando alterações no banco de dados...");
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Novo usuário registrado com sucesso: {Email}, ID: {Id}", email, user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante registro do usuário {Email}: {Message}\nStackTrace: {StackTrace}", 
                    email, ex.Message, ex.StackTrace);
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

        public async Task<User?> UpdateUserAsync(User user)
        {
            try
            {
                var existing = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id && u.IsActive);
                if (existing == null) return null;

                existing.Name = user.Name;
                existing.Email = user.Email;
                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    existing.Password = user.Password.Length > 40 ? user.Password : HashPassword(user.Password);
                }

                await _context.SaveChangesAsync();
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário {UserId}", user.Id);
                return null;
            }
        }

        public async Task<bool> SoftDeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
                if (user == null) return false;
                user.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário {UserId}", id);
                return false;
            }
        }

        public async Task<bool> PromoteToAdminAsync(int id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
                if (user == null) return false;
                user.Role = "Admin";
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao promover usuário a admin {UserId}", id);
                return false;
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

                await _context.Database.EnsureCreatedAsync();

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
