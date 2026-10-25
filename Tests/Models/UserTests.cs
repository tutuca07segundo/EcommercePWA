using EcommercePWA.Models;
using Xunit;

namespace EcommercePWA.Tests.Models
{
    public class UserTests
    {
        [Fact]
        public void User_CreateValidUser_Succeeds()
        {
            // Arrange & Act
            var user = new User
            {
                Name = "João Silva",
                Email = "joao@example.com",
                Password = "hashedpassword"
            };

            // Assert
            Assert.Equal("João Silva", user.Name);
            Assert.Equal("joao@example.com", user.Email);
            Assert.Equal("User", user.Role);
            Assert.True(user.IsActive);
        }

        [Fact]
        public void User_SetAdminRole_SetsRole()
        {
            // Arrange
            var user = new User
            {
                Name = "Admin User",
                Email = "admin@example.com",
                Password = "hashedpassword"
            };

            // Act
            user.Role = "Admin";

            // Assert
            Assert.Equal("Admin", user.Role);
        }

        [Fact]
        public void User_DeactivateUser_SetsInactive()
        {
            // Arrange
            var user = new User
            {
                Name = "João Silva",
                Email = "joao@example.com",
                Password = "hashedpassword",
                IsActive = true
            };

            // Act
            user.IsActive = false;

            // Assert
            Assert.False(user.IsActive);
        }

        [Fact]
        public void User_Collections_Initialized()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.NotNull(user.Orders);
            Assert.NotNull(user.Reviews);
            Assert.NotNull(user.WishlistItems);
        }
    }
}
