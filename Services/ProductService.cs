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
        
        // Advanced Filter Methods
        Task<FilteredProductsResult> FilterProductsAsync(ProductFilterModel filter);
        Task<ProductFilterOptions> GetFilterOptionsAsync();
        Task<List<SearchSuggestion>> GetSearchSuggestionsAsync(string searchTerm, int maxResults = 5);
        Task<List<string>> GetBrandsByCategoryAsync(string category);
        
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
                .Where(p => p.Id != product.Id && p.Category == product.Category)
                .OrderBy(p => p.Name)
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
                
                // Atualizar campos de peso e dimensões para frete
                existingProduct.Weight = product.Weight;
                existingProduct.Width = product.Width;
                existingProduct.Height = product.Height;
                existingProduct.Length = product.Length;

                _logger.LogInformation("Atualizando produto {ProductName} - Peso: {Weight}g, Dimensões: {Width}x{Height}x{Length}cm", 
                    product.Name, product.Weight, product.Width, product.Height, product.Length);

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
                if (await _context.Categories.AnyAsync())
                    return true;

                await InitializeCategoriesAsync();
                await InitializeProductsAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar banco de dados");
                return false;
            }
        }

        private async Task InitializeCategoriesAsync()
        {
            var categories = CreateDefaultCategories();
            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Categorias inicializadas: {CategoryCount}", categories.Count);
        }

        private async Task InitializeProductsAsync()
        {
            if (await _context.Products.AnyAsync())
                return;

            var products = CreateDefaultProducts();
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Produtos inicializados: {ProductCount}", products.Count);
        }

        private static List<Category> CreateDefaultCategories()
        {
            return new List<Category>
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
        }

        private static List<Product> CreateDefaultProducts()
        {
            return new List<Product>
            {
                // Mouses
                CreateProduct("Mouse Gaming Logitech G Pro X Superlight", "Mouse wireless ultra leve com sensor HERO 25K", 599.99m, "Mouses", "Logitech", 15, "Periférico", new[] { "gaming", "wireless", "leve" }, 4.8m, 150, 12, 4, 7),
                CreateProduct("Mouse Razer DeathAdder V3 Pro", "Mouse ergonômico com sensor Focus Pro 30K", 699.99m, "Mouses", "Razer", 8, "Periférico", new[] { "gaming", "ergonômico", "premium" }, 4.7m, 180, 13, 4, 8),
                CreateProduct("Mouse HyperX Pulsefire Haste", "Mouse ultra leve com switches ópticos", 299.99m, "Mouses", "HyperX", 25, "Periférico", new[] { "gaming", "leve", "acessível" }, 4.5m, 120, 11, 3, 6),

                // Teclados
                CreateProduct("Teclado Mecânico Corsair K100 RGB", "Teclado premium com switches Cherry MX", 1299.99m, "Teclados", "Corsair", 5, "Periférico", new[] { "mecânico", "rgb", "premium" }, 4.9m, 1200, 45, 4, 17),
                CreateProduct("Teclado HyperX Alloy Origins Core", "Teclado compacto com switches Blue", 449.99m, "Teclados", "HyperX", 18, "Periférico", new[] { "mecânico", "compacto", "gaming" }, 4.6m),
                CreateProduct("Teclado Logitech G915 TKL", "Teclado wireless com switches GL", 899.99m, "Teclados", "Logitech", 12, "Periférico", new[] { "wireless", "mecânico", "rgb" }, 4.7m),

                // Headsets
                CreateProduct("Headset SteelSeries Arctis Pro", "Headset premium com áudio Hi-Res", 899.99m, "Headsets", "SteelSeries", 10, "Periférico", new[] { "gaming", "áudio", "premium" }, 4.8m, 350, 20, 18, 10),
                CreateProduct("Headset HyperX Cloud Alpha", "Headset com drivers duplos", 399.99m, "Headsets", "HyperX", 22, "Periférico", new[] { "gaming", "confortável", "qualidade" }, 4.6m),
                CreateProduct("Headset Logitech G Pro X", "Headset com microfone Blue VO!CE", 599.99m, "Headsets", "Logitech", 15, "Periférico", new[] { "gaming", "microfone", "profissional" }, 4.7m),

                // Monitores
                CreateProduct("Monitor Samsung Odyssey G9", "Monitor ultrawide 49\" 240Hz", 8999.99m, "Monitores", "Samsung", 3, "Periférico", new[] { "ultrawide", "240hz", "curvo" }, 4.9m),
                CreateProduct("Monitor LG 27GL850-B", "Monitor 27\" 1440p 144Hz Nano IPS", 2499.99m, "Monitores", "LG", 8, "Periférico", new[] { "1440p", "144hz", "nano-ips" }, 4.8m),
                CreateProduct("Monitor AOC C24G1", "Monitor 24\" 1080p 144Hz VA", 899.99m, "Monitores", "AOC", 20, "Periférico", new[] { "1080p", "144hz", "curvo" }, 4.5m),

                // Processadores
                CreateProduct("Intel Core i9-13900K", "Processador 24 cores 5.8GHz", 3499.99m, "Processadores", "Intel", 7, "Peça", new[] { "intel", "i9", "gaming" }, 4.9m),
                CreateProduct("AMD Ryzen 9 7950X", "Processador 16 cores 5.7GHz", 3999.99m, "Processadores", "AMD", 5, "Peça", new[] { "amd", "ryzen", "zen4" }, 4.8m),
                CreateProduct("Intel Core i5-13600K", "Processador 14 cores 5.1GHz", 1899.99m, "Processadores", "Intel", 15, "Peça", new[] { "intel", "i5", "mid-range" }, 4.7m),

                // Placas de Vídeo
                CreateProduct("NVIDIA RTX 4090 Founders Edition", "GPU mais poderosa do mundo", 12999.99m, "Placas de Vídeo", "NVIDIA", 2, "Peça", new[] { "rtx", "4090", "top" }, 5.0m),
                CreateProduct("AMD RX 7900 XTX", "GPU AMD de alta performance", 7999.99m, "Placas de Vídeo", "AMD", 6, "Peça", new[] { "amd", "rx", "7900" }, 4.8m),
                CreateProduct("NVIDIA RTX 4070 Ti", "GPU RTX 40 série mid-range", 4499.99m, "Placas de Vídeo", "NVIDIA", 12, "Peça", new[] { "rtx", "4070", "mid-range" }, 4.6m),

                // Memórias RAM
                CreateProduct("Corsair Dominator Platinum 32GB", "RAM DDR5 6000MHz CL36", 899.99m, "Memórias RAM", "Corsair", 18, "Peça", new[] { "ddr5", "32gb", "6000mhz" }, 4.8m),
                CreateProduct("G.Skill Trident Z5 16GB", "RAM DDR5 6400MHz CL32", 599.99m, "Memórias RAM", "G.Skill", 25, "Peça", new[] { "ddr5", "16gb", "6400mhz" }, 4.7m),
                CreateProduct("Kingston Fury Beast 64GB", "RAM DDR4 3200MHz CL16", 799.99m, "Memórias RAM", "Kingston", 30, "Peça", new[] { "ddr4", "64gb", "3200mhz" }, 4.6m),

                // SSDs
                CreateProduct("Samsung 990 Pro 2TB", "SSD NVMe PCIe 4.0 7450MB/s", 899.99m, "SSDs", "Samsung", 20, "Peça", new[] { "nvme", "pcie4", "2tb" }, 4.9m),
                CreateProduct("WD Black SN850X 1TB", "SSD NVMe PCIe 4.0 7300MB/s", 599.99m, "SSDs", "Western Digital", 28, "Peça", new[] { "nvme", "pcie4", "1tb" }, 4.8m),
                CreateProduct("Crucial P5 Plus 500GB", "SSD NVMe PCIe 4.0 6600MB/s", 299.99m, "SSDs", "Crucial", 35, "Peça", new[] { "nvme", "pcie4", "500gb" }, 4.7m)
            };
        }

        private static Product CreateProduct(string name, string description, decimal price, string category, string brand, int stock, string type, string[] tags, decimal rating, decimal weight = 200m, int width = 15, int height = 10, int length = 20)
        {
            return new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Category = category,
                Brand = brand,
                ImageUrl = "/images/placeholder.jpg",
                StockQuantity = stock,
                Type = type,
                Tags = tags.ToList(),
                Weight = weight,
                Width = width,
                Height = height,
                Length = length
            };
        }

        // Advanced Filter Methods Implementation
        public async Task<FilteredProductsResult> FilterProductsAsync(ProductFilterModel filter)
        {
            try
            {
                IQueryable<Product> query = _context.Products
                    .Include(p => p.Variations)
                    .AsNoTracking();

                // Apply search term filter
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    query = query.Where(p => 
                        EF.Functions.Like(p.Name, $"%{filter.SearchTerm}%") ||
                        EF.Functions.Like(p.Description, $"%{filter.SearchTerm}%") ||
                        EF.Functions.Like(p.Brand, $"%{filter.SearchTerm}%") ||
                        EF.Functions.Like(p.Category, $"%{filter.SearchTerm}%"));
                }

                // Apply category filter
                if (filter.Categories.Any())
                {
                    query = query.Where(p => filter.Categories.Contains(p.Category));
                }

                // Apply brand filter
                if (filter.Brands.Any())
                {
                    query = query.Where(p => filter.Brands.Contains(p.Brand));
                }

                // Apply price range filter
                if (filter.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= filter.MinPrice.Value);
                }
                if (filter.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= filter.MaxPrice.Value);
                }


                // Apply stock filter
                if (filter.InStockOnly == true)
                {
                    query = query.Where(p => p.StockQuantity > 0);
                }

                // Apply sale filter
                if (filter.OnSaleOnly == true)
                {
                    query = query.Where(p => p.HasDiscount);
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply sorting
                query = filter.SortBy.ToLower() switch
                {
                    "name" => filter.SortDirection == "desc" 
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name),
                    "price" => filter.SortDirection == "desc"
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.Name)
                };

                // Apply pagination
                var products = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

                return new FilteredProductsResult
                {
                    Products = products,
                    TotalCount = totalCount,
                    CurrentPage = filter.Page,
                    TotalPages = totalPages,
                    FilterOptions = await GetFilterOptionsAsync()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao filtrar produtos");
                return new FilteredProductsResult();
            }
        }

        public async Task<ProductFilterOptions> GetFilterOptionsAsync()
        {
            try
            {
                var categories = await _context.Products
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                var brands = await _context.Products
                    .Select(p => p.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync();

                var prices = await _context.Products
                    .Select(p => p.Price)
                    .ToListAsync();

                return new ProductFilterOptions
                {
                    AvailableCategories = categories,
                    AvailableBrands = brands,
                    MinPrice = prices.Any() ? prices.Min() : 0,
                    MaxPrice = prices.Any() ? prices.Max() : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter opções de filtro");
                return new ProductFilterOptions();
            }
        }

        public async Task<List<SearchSuggestion>> GetSearchSuggestionsAsync(string searchTerm, int maxResults = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                    return new List<SearchSuggestion>();

                var suggestions = await _context.Products
                    .AsNoTracking()
                    .Where(p => 
                        EF.Functions.Like(p.Name, $"%{searchTerm}%") ||
                        EF.Functions.Like(p.Brand, $"%{searchTerm}%") ||
                        EF.Functions.Like(p.Category, $"%{searchTerm}%"))
                    .OrderBy(p => p.Name)
                    .Take(maxResults)
                    .Select(p => new SearchSuggestion
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Category = p.Category,
                        Brand = p.Brand,
                        Price = p.Price,
                        ImageUrl = p.ImageUrl,
                        Type = p.Type
                    })
                    .ToListAsync();

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sugestões de busca");
                return new List<SearchSuggestion>();
            }
        }

        public async Task<List<string>> GetBrandsByCategoryAsync(string category)
        {
            try
            {
                var brands = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.Category == category)
                    .Select(p => p.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync();

                return brands;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar marcas da categoria: {Category}", category);
                return new List<string>();
            }
        }
    }
}
