using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IProductService
    {
        List<Product> GetAllProducts();
        List<Product> SearchProducts(string searchTerm);
        List<Product> GetProductsByCategory(string category);
        List<Product> SortProducts(List<Product> products, string sortOrder);
        List<Category> GetAllCategories();
        Product? GetProductById(int id);
    }

    public class ProductService : IProductService
    {
        private readonly List<Product> _products;
        private readonly List<Category> _categories;

        public ProductService()
        {
            _categories = InitializeCategories();
            _products = InitializeProducts();
        }

        private List<Category> InitializeCategories()
        {
            return new List<Category>
            {
                new Category { Id = 1, Name = "Mouses", Description = "Mouses gaming e profissionais", IconClass = "fas fa-mouse", Color = "#FF6B6B", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Teclados", Description = "Teclados mecânicos e membrana", IconClass = "fas fa-keyboard", Color = "#4ECDC4", DisplayOrder = 2 },
                new Category { Id = 3, Name = "Headsets", Description = "Fones de ouvido e microfones", IconClass = "fas fa-headphones", Color = "#45B7D1", DisplayOrder = 3 },
                new Category { Id = 4, Name = "Monitores", Description = "Monitores gaming e profissionais", IconClass = "fas fa-tv", Color = "#96CEB4", DisplayOrder = 4 },
                new Category { Id = 5, Name = "Processadores", Description = "CPUs Intel e AMD", IconClass = "fas fa-microchip", Color = "#FFEAA7", DisplayOrder = 5 },
                new Category { Id = 6, Name = "Placas de Vídeo", Description = "GPUs NVIDIA e AMD", IconClass = "fas fa-gamepad", Color = "#DDA0DD", DisplayOrder = 6 },
                new Category { Id = 7, Name = "Memórias RAM", Description = "Memórias DDR4 e DDR5", IconClass = "fas fa-memory", Color = "#98D8C8", DisplayOrder = 7 },
                new Category { Id = 8, Name = "SSDs", Description = "Discos sólidos de alta velocidade", IconClass = "fas fa-hdd", Color = "#F7DC6F", DisplayOrder = 8 }
            };
        }

        private List<Product> InitializeProducts()
        {
            return new List<Product>
            {
                // Mouses
                new Product { Id = 1, Name = "Mouse Gaming Logitech G Pro X Superlight", Description = "Mouse wireless ultra leve com sensor HERO 25K", Price = 599.99m, Category = "Mouses", Brand = "Logitech", ImageUrl = "/images/mouse-logitech-gpro.jpg", StockQuantity = 15, Type = "Periférico", Tags = new List<string> { "gaming", "wireless", "leve" }, Rating = 4.8m, ReviewCount = 127 },
                new Product { Id = 2, Name = "Mouse Razer DeathAdder V3 Pro", Description = "Mouse ergonômico com sensor Focus Pro 30K", Price = 699.99m, Category = "Mouses", Brand = "Razer", ImageUrl = "/images/mouse-razer-deathadder.jpg", StockQuantity = 8, Type = "Periférico", Tags = new List<string> { "gaming", "ergonômico", "premium" }, Rating = 4.7m, ReviewCount = 89 },
                new Product { Id = 3, Name = "Mouse HyperX Pulsefire Haste", Description = "Mouse ultra leve com switches ópticos", Price = 299.99m, Category = "Mouses", Brand = "HyperX", ImageUrl = "/images/mouse-hyperx-haste.jpg", StockQuantity = 25, Type = "Periférico", Tags = new List<string> { "gaming", "leve", "acessível" }, Rating = 4.5m, ReviewCount = 156 },

                // Teclados
                new Product { Id = 4, Name = "Teclado Mecânico Corsair K100 RGB", Description = "Teclado premium com switches Cherry MX", Price = 1299.99m, Category = "Teclados", Brand = "Corsair", ImageUrl = "/images/teclado-corsair-k100.jpg", StockQuantity = 5, Type = "Periférico", Tags = new List<string> { "mecânico", "rgb", "premium" }, Rating = 4.9m, ReviewCount = 67 },
                new Product { Id = 5, Name = "Teclado HyperX Alloy Origins Core", Description = "Teclado compacto com switches Blue", Price = 449.99m, Category = "Teclados", Brand = "HyperX", ImageUrl = "/images/teclado-hyperx-alloy.jpg", StockQuantity = 18, Type = "Periférico", Tags = new List<string> { "mecânico", "compacto", "gaming" }, Rating = 4.6m, ReviewCount = 203 },
                new Product { Id = 6, Name = "Teclado Logitech G915 TKL", Description = "Teclado wireless com switches GL", Price = 899.99m, Category = "Teclados", Brand = "Logitech", ImageUrl = "/images/teclado-logitech-g915.jpg", StockQuantity = 12, Type = "Periférico", Tags = new List<string> { "wireless", "mecânico", "rgb" }, Rating = 4.7m, ReviewCount = 134 },

                // Headsets
                new Product { Id = 7, Name = "Headset SteelSeries Arctis Pro", Description = "Headset premium com áudio Hi-Res", Price = 899.99m, Category = "Headsets", Brand = "SteelSeries", ImageUrl = "/images/headset-steelseries-arctis.jpg", StockQuantity = 10, Type = "Periférico", Tags = new List<string> { "gaming", "áudio", "premium" }, Rating = 4.8m, ReviewCount = 89 },
                new Product { Id = 8, Name = "Headset HyperX Cloud Alpha", Description = "Headset com drivers duplos", Price = 399.99m, Category = "Headsets", Brand = "HyperX", ImageUrl = "/images/headset-hyperx-cloud.jpg", StockQuantity = 22, Type = "Periférico", Tags = new List<string> { "gaming", "confortável", "qualidade" }, Rating = 4.6m, ReviewCount = 267 },
                new Product { Id = 9, Name = "Headset Logitech G Pro X", Description = "Headset com microfone Blue VO!CE", Price = 599.99m, Category = "Headsets", Brand = "Logitech", ImageUrl = "/images/headset-logitech-gpro.jpg", StockQuantity = 15, Type = "Periférico", Tags = new List<string> { "gaming", "microfone", "profissional" }, Rating = 4.7m, ReviewCount = 145 },

                // Monitores
                new Product { Id = 10, Name = "Monitor Samsung Odyssey G9", Description = "Monitor ultrawide 49\" 240Hz", Price = 8999.99m, Category = "Monitores", Brand = "Samsung", ImageUrl = "/images/monitor-samsung-g9.jpg", StockQuantity = 3, Type = "Periférico", Tags = new List<string> { "ultrawide", "240hz", "curvo" }, Rating = 4.9m, ReviewCount = 45 },
                new Product { Id = 11, Name = "Monitor LG 27GL850-B", Description = "Monitor 27\" 1440p 144Hz Nano IPS", Price = 2499.99m, Category = "Monitores", Brand = "LG", ImageUrl = "/images/monitor-lg-27gl850.jpg", StockQuantity = 8, Type = "Periférico", Tags = new List<string> { "1440p", "144hz", "nano-ips" }, Rating = 4.8m, ReviewCount = 178 },
                new Product { Id = 12, Name = "Monitor AOC C24G1", Description = "Monitor 24\" 1080p 144Hz VA", Price = 899.99m, Category = "Monitores", Brand = "AOC", ImageUrl = "/images/monitor-aoc-c24g1.jpg", StockQuantity = 20, Type = "Periférico", Tags = new List<string> { "1080p", "144hz", "curvo" }, Rating = 4.5m, ReviewCount = 234 },

                // Processadores
                new Product { Id = 13, Name = "Intel Core i9-13900K", Description = "Processador 24 cores 5.8GHz", Price = 3499.99m, Category = "Processadores", Brand = "Intel", ImageUrl = "/images/processador-intel-i9.jpg", StockQuantity = 7, Type = "Peça", Tags = new List<string> { "intel", "i9", "gaming" }, Rating = 4.9m, ReviewCount = 56 },
                new Product { Id = 14, Name = "AMD Ryzen 9 7950X", Description = "Processador 16 cores 5.7GHz", Price = 3999.99m, Category = "Processadores", Brand = "AMD", ImageUrl = "/images/processador-amd-ryzen9.jpg", StockQuantity = 5, Type = "Peça", Tags = new List<string> { "amd", "ryzen", "zen4" }, Rating = 4.8m, ReviewCount = 43 },
                new Product { Id = 15, Name = "Intel Core i5-13600K", Description = "Processador 14 cores 5.1GHz", Price = 1899.99m, Category = "Processadores", Brand = "Intel", ImageUrl = "/images/processador-intel-i5.jpg", StockQuantity = 15, Type = "Peça", Tags = new List<string> { "intel", "i5", "mid-range" }, Rating = 4.7m, ReviewCount = 89 },

                // Placas de Vídeo
                new Product { Id = 16, Name = "NVIDIA RTX 4090 Founders Edition", Description = "GPU mais poderosa do mundo", Price = 12999.99m, Category = "Placas de Vídeo", Brand = "NVIDIA", ImageUrl = "/images/gpu-rtx4090.jpg", StockQuantity = 2, Type = "Peça", Tags = new List<string> { "rtx", "4090", "top" }, Rating = 5.0m, ReviewCount = 23 },
                new Product { Id = 17, Name = "AMD RX 7900 XTX", Description = "GPU AMD de alta performance", Price = 7999.99m, Category = "Placas de Vídeo", Brand = "AMD", ImageUrl = "/images/gpu-rx7900xtx.jpg", StockQuantity = 6, Type = "Peça", Tags = new List<string> { "amd", "rx", "7900" }, Rating = 4.8m, ReviewCount = 67 },
                new Product { Id = 18, Name = "NVIDIA RTX 4070 Ti", Description = "GPU RTX 40 série mid-range", Price = 4499.99m, Category = "Placas de Vídeo", Brand = "NVIDIA", ImageUrl = "/images/gpu-rtx4070ti.jpg", StockQuantity = 12, Type = "Peça", Tags = new List<string> { "rtx", "4070", "mid-range" }, Rating = 4.6m, ReviewCount = 134 },

                // Memórias RAM
                new Product { Id = 19, Name = "Corsair Dominator Platinum 32GB", Description = "RAM DDR5 6000MHz CL36", Price = 899.99m, Category = "Memórias RAM", Brand = "Corsair", ImageUrl = "/images/ram-corsair-dominator.jpg", StockQuantity = 18, Type = "Peça", Tags = new List<string> { "ddr5", "32gb", "6000mhz" }, Rating = 4.8m, ReviewCount = 78 },
                new Product { Id = 20, Name = "G.Skill Trident Z5 16GB", Description = "RAM DDR5 6400MHz CL32", Price = 599.99m, Category = "Memórias RAM", Brand = "G.Skill", ImageUrl = "/images/ram-gskill-trident.jpg", StockQuantity = 25, Type = "Peça", Tags = new List<string> { "ddr5", "16gb", "6400mhz" }, Rating = 4.7m, ReviewCount = 156 },
                new Product { Id = 21, Name = "Kingston Fury Beast 64GB", Description = "RAM DDR4 3200MHz CL16", Price = 799.99m, Category = "Memórias RAM", Brand = "Kingston", ImageUrl = "/images/ram-kingston-fury.jpg", StockQuantity = 30, Type = "Peça", Tags = new List<string> { "ddr4", "64gb", "3200mhz" }, Rating = 4.6m, ReviewCount = 89 },

                // SSDs
                new Product { Id = 22, Name = "Samsung 990 Pro 2TB", Description = "SSD NVMe PCIe 4.0 7450MB/s", Price = 899.99m, Category = "SSDs", Brand = "Samsung", ImageUrl = "/images/ssd-samsung-990pro.jpg", StockQuantity = 20, Type = "Peça", Tags = new List<string> { "nvme", "pcie4", "2tb" }, Rating = 4.9m, ReviewCount = 234 },
                new Product { Id = 23, Name = "WD Black SN850X 1TB", Description = "SSD NVMe PCIe 4.0 7300MB/s", Price = 599.99m, Category = "SSDs", Brand = "Western Digital", ImageUrl = "/images/ssd-wd-black.jpg", StockQuantity = 28, Type = "Peça", Tags = new List<string> { "nvme", "pcie4", "1tb" }, Rating = 4.8m, ReviewCount = 167 },
                new Product { Id = 24, Name = "Crucial P5 Plus 500GB", Description = "SSD NVMe PCIe 4.0 6600MB/s", Price = 299.99m, Category = "SSDs", Brand = "Crucial", ImageUrl = "/images/ssd-crucial-p5.jpg", StockQuantity = 35, Type = "Peça", Tags = new List<string> { "nvme", "pcie4", "500gb" }, Rating = 4.7m, ReviewCount = 189 }
            };
        }

        public List<Product> GetAllProducts()
        {
            return _products;
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _products;

            return _products.Where(p => 
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Brand.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Tags.Any(t => t.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        public List<Product> GetProductsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return _products;

            return _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Product> SortProducts(List<Product> products, string sortOrder)
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

        public List<Category> GetAllCategories()
        {
            return _categories;
        }

        public Product? GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }
    }
}
