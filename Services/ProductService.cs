using EcommercePWA.Models;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<List<Product>> SearchProductsAsync(string searchTerm);
        Task<List<Product>> GetProductsByCategoryAsync(string category);
        List<Product> SortProducts(List<Product> products, string sortOrder);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<Product>> GetRelatedProductsAsync(Product product, int count);
        Task<List<ProductVariation>> GetProductVariationsAsync(int productId);
        
        // CRUD Operations
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<ProductVariation> CreateProductVariationAsync(ProductVariation variation);
        Task<ProductVariation> UpdateProductVariationAsync(ProductVariation variation);
        Task<bool> DeleteProductVariationAsync(int id);
        Task<bool> InitializeDatabaseAsync();
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Variations)
                    .Include(p => p.Reviews)
                    .Include(p => p.ProductDiscounts)
                    .ThenInclude(pd => pd.Discount)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os produtos");
                return new List<Product>();
            }
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllProductsAsync();

                return await _context.Products
                    .Include(p => p.Variations)
                    .Include(p => p.Reviews)
                    .Where(p => 
                        p.Name.Contains(searchTerm) ||
                        p.Description.Contains(searchTerm) ||
                        p.Brand.Contains(searchTerm) ||
                        p.Category.Contains(searchTerm) ||
                        p.Tags.Any(t => t.Contains(searchTerm))
                    )
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos com termo: {SearchTerm}", searchTerm);
                return new List<Product>();
            }
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            try
        {
            if (string.IsNullOrWhiteSpace(category))
                    return await GetAllProductsAsync();

                return await _context.Products
                    .Include(p => p.Variations)
                    .Include(p => p.Reviews)
                    .Where(p => p.Category.Equals(category))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos da categoria: {Category}", category);
                return new List<Product>();
            }
        }

        public List<Product> SortProducts(List<Product> products, string sortOrder)
        {
            try
        {
            return sortOrder switch
            {
                "price_asc" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                "name" => products.OrderBy(p => p.Name).ToList(),
                "rating" => products.OrderByDescending(p => p.Rating).ToList(),
                "newest" => products.OrderByDescending(p => p.CreatedAt).ToList(),
                _ => products.OrderBy(p => p.Name).ToList()
            };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ordenar produtos");
                return products;
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                return await _context.Categories
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todas as categorias");
                return new List<Category>();
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Variations)
                    .Include(p => p.Reviews)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produto com ID: {ProductId}", id);
                return null;
            }
        }

        public async Task<List<Product>> GetRelatedProductsAsync(Product product, int count)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Variations)
                    .Include(p => p.Reviews)
                .Where(p => p.Id != product.Id && p.Category == product.Category)
                .OrderByDescending(p => p.Rating)
                .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos relacionados");
                return new List<Product>();
            }
        }

        public async Task<List<ProductVariation>> GetProductVariationsAsync(int productId)
        {
            try
            {
                return await _context.ProductVariations
                    .Where(v => v.ProductId == productId && v.IsAvailable)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar variações do produto: {ProductId}", productId);
                return new List<ProductVariation>();
            }
        }

        // CRUD Operations
        public async Task<Product> CreateProductAsync(Product product)
        {
            try
            {
                product.CreatedAt = DateTime.Now;
                
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Produto criado com sucesso: {ProductName}", product.Name);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar produto: {ProductName}", product.Name);
                throw;
            }
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(product.Id);
                if (existingProduct == null)
                    throw new ArgumentException("Produto não encontrado");

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Category = product.Category;
                existingProduct.Brand = product.Brand;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.Type = product.Type;
                existingProduct.Tags = product.Tags;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Produto atualizado com sucesso: {ProductName}", product.Name);
                return existingProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar produto: {ProductId}", product.Id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return false;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Produto removido com sucesso: {ProductId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover produto: {ProductId}", id);
                return false;
            }
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Categoria criada com sucesso: {CategoryName}", category.Name);
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria: {CategoryName}", category.Name);
                throw;
            }
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            try
            {
                var existingCategory = await _context.Categories.FindAsync(category.Id);
                if (existingCategory == null)
                    throw new ArgumentException("Categoria não encontrada");

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.IconClass = category.IconClass;
                existingCategory.Color = category.Color;
                existingCategory.DisplayOrder = category.DisplayOrder;
                existingCategory.IsActive = category.IsActive;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Categoria atualizada com sucesso: {CategoryName}", category.Name);
                return existingCategory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar categoria: {CategoryId}", category.Id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return false;

                // Verificar se há produtos usando esta categoria
                var productsInCategory = await _context.Products.AnyAsync(p => p.Category == category.Name);
                if (productsInCategory)
                    throw new InvalidOperationException("Não é possível remover uma categoria que possui produtos");

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Categoria removida com sucesso: {CategoryId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover categoria: {CategoryId}", id);
                throw;
            }
        }

        public async Task<ProductVariation> CreateProductVariationAsync(ProductVariation variation)
        {
            try
            {
                _context.ProductVariations.Add(variation);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Variação criada com sucesso para produto: {ProductId}", variation.ProductId);
                return variation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar variação para produto: {ProductId}", variation.ProductId);
                throw;
            }
        }

        public async Task<ProductVariation> UpdateProductVariationAsync(ProductVariation variation)
        {
            try
            {
                var existingVariation = await _context.ProductVariations.FindAsync(variation.Id);
                if (existingVariation == null)
                    throw new ArgumentException("Variação não encontrada");

                existingVariation.Name = variation.Name;
                existingVariation.Type = variation.Type;
                existingVariation.ColorHex = variation.ColorHex;
                existingVariation.PriceModifier = variation.PriceModifier;
                existingVariation.IsAvailable = variation.IsAvailable;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Variação atualizada com sucesso: {VariationId}", variation.Id);
                return existingVariation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar variação: {VariationId}", variation.Id);
                throw;
            }
        }

        public async Task<bool> DeleteProductVariationAsync(int id)
        {
            try
            {
                var variation = await _context.ProductVariations.FindAsync(id);
                if (variation == null)
                    return false;

                _context.ProductVariations.Remove(variation);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Variação removida com sucesso: {VariationId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover variação: {VariationId}", id);
                return false;
            }
        }

        public async Task<bool> InitializeDatabaseAsync()
        {
            try
            {
                // Verificar se já existem categorias
                if (await _context.Categories.AnyAsync())
                    return true;

                var categories = new List<Category>
                {
                    new Category { Name = "Mouses", Description = "Mouses gaming e profissionais", IconClass = "fas fa-mouse", Color = "#FF6B6B", DisplayOrder = 1 },
                    new Category { Name = "Teclados", Description = "Teclados mecânicos e membrana", IconClass = "fas fa-keyboard", Color = "#4ECDC4", DisplayOrder = 2 },
                    new Category { Name = "Headsets", Description = "Fones de ouvido e microfones", IconClass = "fas fa-headphones", Color = "#45B7D1", DisplayOrder = 3 },
                    new Category { Name = "Monitores", Description = "Monitores gaming e profissionais", IconClass = "fas fa-tv", Color = "#96CEB4", DisplayOrder = 4 },
                    new Category { Name = "Processadores", Description = "CPUs Intel e AMD", IconClass = "fas fa-microchip", Color = "#FFEAA7", DisplayOrder = 5 },
                    new Category { Name = "Placas de Vídeo", Description = "GPUs NVIDIA e AMD", IconClass = "fas fa-gamepad", Color = "#DDA0DD", DisplayOrder = 6 },
                    new Category { Name = "Memórias RAM", Description = "Memórias DDR4 e DDR5", IconClass = "fas fa-memory", Color = "#98D8C8", DisplayOrder = 7 },
                    new Category { Name = "SSDs", Description = "Discos sólidos de alta velocidade", IconClass = "fas fa-hdd", Color = "#F7DC6F", DisplayOrder = 8 }
                };

                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();

                // Verificar se já existem produtos
                if (await _context.Products.AnyAsync())
                    return true;

                var products = new List<Product>
                {
                    // Mouses
                    new Product { Name = "Mouse Gaming Logitech G Pro X Superlight", Description = "Mouse wireless ultra leve com sensor HERO 25K", Price = 599.99m, Category = "Mouses", Brand = "Logitech", ImageUrl = "/images/placeholder.jpg", StockQuantity = 15, Type = "Periférico", Tags = new List<string> { "gaming", "wireless", "leve" }, Rating = 4.8m },
                    new Product { Name = "Mouse Razer DeathAdder V3 Pro", Description = "Mouse ergonômico com sensor Focus Pro 30K", Price = 699.99m, Category = "Mouses", Brand = "Razer", ImageUrl = "/images/placeholder.jpg", StockQuantity = 8, Type = "Periférico", Tags = new List<string> { "gaming", "ergonômico", "premium" }, Rating = 4.7m },
                    new Product { Name = "Mouse HyperX Pulsefire Haste", Description = "Mouse ultra leve com switches ópticos", Price = 299.99m, Category = "Mouses", Brand = "HyperX", ImageUrl = "/images/placeholder.jpg", StockQuantity = 25, Type = "Periférico", Tags = new List<string> { "gaming", "leve", "acessível" }, Rating = 4.5m },

                    // Teclados
                    new Product { Name = "Teclado Mecânico Corsair K100 RGB", Description = "Teclado premium com switches Cherry MX", Price = 1299.99m, Category = "Teclados", Brand = "Corsair", ImageUrl = "/images/placeholder.jpg", StockQuantity = 5, Type = "Periférico", Tags = new List<string> { "mecânico", "rgb", "premium" }, Rating = 4.9m },
                    new Product { Name = "Teclado HyperX Alloy Origins Core", Description = "Teclado compacto com switches Blue", Price = 449.99m, Category = "Teclados", Brand = "HyperX", ImageUrl = "/images/placeholder.jpg", StockQuantity = 18, Type = "Periférico", Tags = new List<string> { "mecânico", "compacto", "gaming" }, Rating = 4.6m },
                    new Product { Name = "Teclado Logitech G915 TKL", Description = "Teclado wireless com switches GL", Price = 899.99m, Category = "Teclados", Brand = "Logitech", ImageUrl = "/images/placeholder.jpg", StockQuantity = 12, Type = "Periférico", Tags = new List<string> { "wireless", "mecânico", "rgb" }, Rating = 4.7m },

                    // Headsets
                    new Product { Name = "Headset SteelSeries Arctis Pro", Description = "Headset premium com áudio Hi-Res", Price = 899.99m, Category = "Headsets", Brand = "SteelSeries", ImageUrl = "/images/placeholder.jpg", StockQuantity = 10, Type = "Periférico", Tags = new List<string> { "gaming", "áudio", "premium" }, Rating = 4.8m },
                    new Product { Name = "Headset HyperX Cloud Alpha", Description = "Headset com drivers duplos", Price = 399.99m, Category = "Headsets", Brand = "HyperX", ImageUrl = "/images/placeholder.jpg", StockQuantity = 22, Type = "Periférico", Tags = new List<string> { "gaming", "confortável", "qualidade" }, Rating = 4.6m },
                    new Product { Name = "Headset Logitech G Pro X", Description = "Headset com microfone Blue VO!CE", Price = 599.99m, Category = "Headsets", Brand = "Logitech", ImageUrl = "/images/placeholder.jpg", StockQuantity = 15, Type = "Periférico", Tags = new List<string> { "gaming", "microfone", "profissional" }, Rating = 4.7m },

                    // Monitores
                    new Product { Name = "Monitor Samsung Odyssey G9", Description = "Monitor ultrawide 49\" 240Hz", Price = 8999.99m, Category = "Monitores", Brand = "Samsung", ImageUrl = "/images/placeholder.jpg", StockQuantity = 3, Type = "Periférico", Tags = new List<string> { "ultrawide", "240hz", "curvo" }, Rating = 4.9m },
                    new Product { Name = "Monitor LG 27GL850-B", Description = "Monitor 27\" 1440p 144Hz Nano IPS", Price = 2499.99m, Category = "Monitores", Brand = "LG", ImageUrl = "/images/placeholder.jpg", StockQuantity = 8, Type = "Periférico", Tags = new List<string> { "1440p", "144hz", "nano-ips" }, Rating = 4.8m },
                    new Product { Name = "Monitor AOC C24G1", Description = "Monitor 24\" 1080p 144Hz VA", Price = 899.99m, Category = "Monitores", Brand = "AOC", ImageUrl = "/images/placeholder.jpg", StockQuantity = 20, Type = "Periférico", Tags = new List<string> { "1080p", "144hz", "curvo" }, Rating = 4.5m },

                    // Processadores
                    new Product { Name = "Intel Core i9-13900K", Description = "Processador 24 cores 5.8GHz", Price = 3499.99m, Category = "Processadores", Brand = "Intel", ImageUrl = "/images/placeholder.jpg", StockQuantity = 7, Type = "Peça", Tags = new List<string> { "intel", "i9", "gaming" }, Rating = 4.9m },
                    new Product { Name = "AMD Ryzen 9 7950X", Description = "Processador 16 cores 5.7GHz", Price = 3999.99m, Category = "Processadores", Brand = "AMD", ImageUrl = "/images/placeholder.jpg", StockQuantity = 5, Type = "Peça", Tags = new List<string> { "amd", "ryzen", "zen4" }, Rating = 4.8m },
                    new Product { Name = "Intel Core i5-13600K", Description = "Processador 14 cores 5.1GHz", Price = 1899.99m, Category = "Processadores", Brand = "Intel", ImageUrl = "/images/placeholder.jpg", StockQuantity = 15, Type = "Peça", Tags = new List<string> { "intel", "i5", "mid-range" }, Rating = 4.7m },

                    // Placas de Vídeo
                    new Product { Name = "NVIDIA RTX 4090 Founders Edition", Description = "GPU mais poderosa do mundo", Price = 12999.99m, Category = "Placas de Vídeo", Brand = "NVIDIA", ImageUrl = "/images/placeholder.jpg", StockQuantity = 2, Type = "Peça", Tags = new List<string> { "rtx", "4090", "top" }, Rating = 5.0m },
                    new Product { Name = "AMD RX 7900 XTX", Description = "GPU AMD de alta performance", Price = 7999.99m, Category = "Placas de Vídeo", Brand = "AMD", ImageUrl = "/images/placeholder.jpg", StockQuantity = 6, Type = "Peça", Tags = new List<string> { "amd", "rx", "7900" }, Rating = 4.8m },
                    new Product { Name = "NVIDIA RTX 4070 Ti", Description = "GPU RTX 40 série mid-range", Price = 4499.99m, Category = "Placas de Vídeo", Brand = "NVIDIA", ImageUrl = "/images/placeholder.jpg", StockQuantity = 12, Type = "Peça", Tags = new List<string> { "rtx", "4070", "mid-range" }, Rating = 4.6m },

                    // Memórias RAM
                    new Product { Name = "Corsair Dominator Platinum 32GB", Description = "RAM DDR5 6000MHz CL36", Price = 899.99m, Category = "Memórias RAM", Brand = "Corsair", ImageUrl = "/images/placeholder.jpg", StockQuantity = 18, Type = "Peça", Tags = new List<string> { "ddr5", "32gb", "6000mhz" }, Rating = 4.8m },
                    new Product { Name = "G.Skill Trident Z5 16GB", Description = "RAM DDR5 6400MHz CL32", Price = 599.99m, Category = "Memórias RAM", Brand = "G.Skill", ImageUrl = "/images/placeholder.jpg", StockQuantity = 25, Type = "Peça", Tags = new List<string> { "ddr5", "16gb", "6400mhz" }, Rating = 4.7m },
                    new Product { Name = "Kingston Fury Beast 64GB", Description = "RAM DDR4 3200MHz CL16", Price = 799.99m, Category = "Memórias RAM", Brand = "Kingston", ImageUrl = "/images/placeholder.jpg", StockQuantity = 30, Type = "Peça", Tags = new List<string> { "ddr4", "64gb", "3200mhz" }, Rating = 4.6m },

                    // SSDs
                    new Product { Name = "Samsung 990 Pro 2TB", Description = "SSD NVMe PCIe 4.0 7450MB/s", Price = 899.99m, Category = "SSDs", Brand = "Samsung", ImageUrl = "/images/placeholder.jpg", StockQuantity = 20, Type = "Peça", Tags = new List<string> { "nvme", "pcie4", "2tb" }, Rating = 4.9m },
                    new Product { Name = "WD Black SN850X 1TB", Description = "SSD NVMe PCIe 4.0 7300MB/s", Price = 599.99m, Category = "SSDs", Brand = "Western Digital", ImageUrl = "/images/placeholder.jpg", StockQuantity = 28, Type = "Peça", Tags = new List<string> { "nvme", "pcie4", "1tb" }, Rating = 4.8m },
                    new Product { Name = "Crucial P5 Plus 500GB", Description = "SSD NVMe PCIe 4.0 6600MB/s", Price = 299.99m, Category = "SSDs", Brand = "Crucial", ImageUrl = "/images/placeholder.jpg", StockQuantity = 35, Type = "Peça", Tags = new List<string> { "nvme", "pcie4", "500gb" }, Rating = 4.7m }
                };

                _context.Products.AddRange(products);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Banco de dados inicializado com sucesso com {ProductCount} produtos e {CategoryCount} categorias", products.Count, categories.Count);
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
