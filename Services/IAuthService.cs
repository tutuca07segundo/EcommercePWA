using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IAuthService
    {
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User?> RegisterAsync(string name, string email, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> UpdateUserAsync(User user);
    Task<bool> SoftDeleteUserAsync(int id);
    Task<bool> PromoteToAdminAsync(int id);
    bool VerifyPassword(string password, string hashedPassword);
    string HashPassword(string password);
    Task<bool> CreateAdminUserAsync();
    Task<bool> InitializeDatabaseAsync();
    }
}
