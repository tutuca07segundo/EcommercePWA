using EcommercePWA.Data;
using EcommercePWA.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Services
{
    public class DataSeederService
    {
        private readonly ApplicationDbContext _context;

        public DataSeederService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedSubcategoriesAsync()
        {

            if (await _context.Subcategories.AnyAsync())
            {
                return; // Dados já foram inseridos
            }

            var categories = await _context.Categories.ToListAsync();
            var subcategoriesToAdd = new List<Subcategory>();

            foreach (var category in categories)
            {
                switch (category.Name.ToLower())
                {
                    case "mouses":
                        subcategoriesToAdd.AddRange(new[]
                        {
                            new Subcategory { Name = "Razer", Description = "Produtos da marca Razer", CategoryId = category.Id, IsActive = true, DisplayOrder = 1 },
                            new Subcategory { Name = "Logitech", Description = "Produtos da marca Logitech", CategoryId = category.Id, IsActive = true, DisplayOrder = 2 },
                            new Subcategory { Name = "Redragon", Description = "Produtos da marca Redragon", CategoryId = category.Id, IsActive = true, DisplayOrder = 3 },
                            new Subcategory { Name = "Corsair", Description = "Produtos da marca Corsair", CategoryId = category.Id, IsActive = true, DisplayOrder = 4 },
                            new Subcategory { Name = "SteelSeries", Description = "Produtos da marca SteelSeries", CategoryId = category.Id, IsActive = true, DisplayOrder = 5 }
                        });
                        break;

                    case "teclados":
                        subcategoriesToAdd.AddRange(new[]
                        {
                            new Subcategory { Name = "Razer", Description = "Teclados Razer", CategoryId = category.Id, IsActive = true, DisplayOrder = 1 },
                            new Subcategory { Name = "Logitech", Description = "Teclados Logitech", CategoryId = category.Id, IsActive = true, DisplayOrder = 2 },
                            new Subcategory { Name = "Redragon", Description = "Teclados Redragon", CategoryId = category.Id, IsActive = true, DisplayOrder = 3 },
                            new Subcategory { Name = "Corsair", Description = "Teclados Corsair", CategoryId = category.Id, IsActive = true, DisplayOrder = 4 },
                            new Subcategory { Name = "HyperX", Description = "Teclados HyperX", CategoryId = category.Id, IsActive = true, DisplayOrder = 5 }
                        });
                        break;

                    case "headsets":
                        subcategoriesToAdd.AddRange(new[]
                        {
                            new Subcategory { Name = "Razer", Description = "Headsets Razer", CategoryId = category.Id, IsActive = true, DisplayOrder = 1 },
                            new Subcategory { Name = "Logitech", Description = "Headsets Logitech", CategoryId = category.Id, IsActive = true, DisplayOrder = 2 },
                            new Subcategory { Name = "HyperX", Description = "Headsets HyperX", CategoryId = category.Id, IsActive = true, DisplayOrder = 3 },
                            new Subcategory { Name = "SteelSeries", Description = "Headsets SteelSeries", CategoryId = category.Id, IsActive = true, DisplayOrder = 4 },
                            new Subcategory { Name = "Corsair", Description = "Headsets Corsair", CategoryId = category.Id, IsActive = true, DisplayOrder = 5 }
                        });
                        break;

                    case "monitores":
                        subcategoriesToAdd.AddRange(new[]
                        {
                            new Subcategory { Name = "ASUS", Description = "Monitores ASUS", CategoryId = category.Id, IsActive = true, DisplayOrder = 1 },
                            new Subcategory { Name = "LG", Description = "Monitores LG", CategoryId = category.Id, IsActive = true, DisplayOrder = 2 },
                            new Subcategory { Name = "Samsung", Description = "Monitores Samsung", CategoryId = category.Id, IsActive = true, DisplayOrder = 3 },
                            new Subcategory { Name = "AOC", Description = "Monitores AOC", CategoryId = category.Id, IsActive = true, DisplayOrder = 4 },
                            new Subcategory { Name = "Dell", Description = "Monitores Dell", CategoryId = category.Id, IsActive = true, DisplayOrder = 5 }
                        });
                        break;

                    case "processadores":
                        subcategoriesToAdd.AddRange(new[]
                        {
                            new Subcategory { Name = "Intel", Description = "Processadores Intel", CategoryId = category.Id, IsActive = true, DisplayOrder = 1 },
                            new Subcategory { Name = "AMD", Description = "Processadores AMD", CategoryId = category.Id, IsActive = true, DisplayOrder = 2 }
                        });
                        break;

                    case "placas de vídeo":
                    case "placas de video":
                        subcategoriesToAdd.AddRange(new[]
                        {
                            new Subcategory { Name = "NVIDIA", Description = "Placas de vídeo NVIDIA", CategoryId = category.Id, IsActive = true, DisplayOrder = 1 },
                            new Subcategory { Name = "AMD", Description = "Placas de vídeo AMD", CategoryId = category.Id, IsActive = true, DisplayOrder = 2 },
                            new Subcategory { Name = "ASUS", Description = "Placas de vídeo ASUS", CategoryId = category.Id, IsActive = true, DisplayOrder = 3 },
                            new Subcategory { Name = "MSI", Description = "Placas de vídeo MSI", CategoryId = category.Id, IsActive = true, DisplayOrder = 4 },
                            new Subcategory { Name = "Gigabyte", Description = "Placas de vídeo Gigabyte", CategoryId = category.Id, IsActive = true, DisplayOrder = 5 }
                        });
                        break;

                    case "memórias ram":
                    case "memorias ram":
                        subcategoriesToAdd.AddRange(new[]
                        {
                            new Subcategory { Name = "Corsair", Description = "Memórias RAM Corsair", CategoryId = category.Id, IsActive = true, DisplayOrder = 1 },
                            new Subcategory { Name = "Kingston", Description = "Memórias RAM Kingston", CategoryId = category.Id, IsActive = true, DisplayOrder = 2 },
                            new Subcategory { Name = "G.Skill", Description = "Memórias RAM G.Skill", CategoryId = category.Id, IsActive = true, DisplayOrder = 3 },
                            new Subcategory { Name = "HyperX", Description = "Memórias RAM HyperX", CategoryId = category.Id, IsActive = true, DisplayOrder = 4 }
                        });
                        break;

                    case "ssds":
                        subcategoriesToAdd.AddRange(new[]
                        {
                            new Subcategory { Name = "Samsung", Description = "SSDs Samsung", CategoryId = category.Id, IsActive = true, DisplayOrder = 1 },
                            new Subcategory { Name = "Kingston", Description = "SSDs Kingston", CategoryId = category.Id, IsActive = true, DisplayOrder = 2 },
                            new Subcategory { Name = "WD", Description = "SSDs Western Digital", CategoryId = category.Id, IsActive = true, DisplayOrder = 3 },
                            new Subcategory { Name = "Crucial", Description = "SSDs Crucial", CategoryId = category.Id, IsActive = true, DisplayOrder = 4 }
                        });
                        break;
                }
            }

            if (subcategoriesToAdd.Any())
            {
                await _context.Subcategories.AddRangeAsync(subcategoriesToAdd);
                await _context.SaveChangesAsync();
            }
        }
    }
}
