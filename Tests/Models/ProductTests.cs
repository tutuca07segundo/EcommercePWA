using EcommercePWA.Models;
using Xunit;

namespace EcommercePWA.Tests.Models
{
    public class ProductTests
    {
        [Fact]
        public void Product_CreateValidProduct_Succeeds()
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Mouse Gaming",
                Description = "Mouse de alta precisão",
                Price = 150m,
                Category = "Mouses",
                Brand = "Logitech",
                StockQuantity = 10
            };

            // Assert
            Assert.Equal("Mouse Gaming", product.Name);
            Assert.Equal(150m, product.Price);
            Assert.Equal("Mouses", product.Category);
            Assert.True(product.IsAvailable);
        }

        [Fact]
        public void Product_SetDiscount_SetsProperties()
        {
            // Arrange
            var product = new Product
            {
                Name = "Mouse Gaming",
                Price = 150m,
                Category = "Mouses",
                Brand = "Logitech"
            };

            // Act
            product.DiscountedPrice = 120m;
            product.HasDiscount = true;

            // Assert
            Assert.Equal(120m, product.DiscountedPrice);
            Assert.True(product.HasDiscount);
        }

        [Fact]
        public void Product_VariationsList_Initialized()
        {
            // Arrange & Act
            var product = new Product();

            // Assert
            Assert.NotNull(product.Variations);
            Assert.Empty(product.Variations);
        }

        [Fact]
        public void Product_TagsList_Initialized()
        {
            // Arrange & Act
            var product = new Product();

            // Assert
            Assert.NotNull(product.Tags);
            Assert.Empty(product.Tags);
        }
    }
}
