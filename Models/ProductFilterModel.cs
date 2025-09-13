using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class ProductFilterModel
    {
        public string? SearchTerm { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<string> Brands { get; set; } = new();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public bool? InStockOnly { get; set; }
        public bool? OnSaleOnly { get; set; }
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class FilteredProductsResult
    {
        public List<Product> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public ProductFilterOptions FilterOptions { get; set; } = new();
    }

    public class ProductFilterOptions
    {
        public List<string> AvailableCategories { get; set; } = new();
        public List<string> AvailableBrands { get; set; } = new();
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public List<decimal> AvailableRatings { get; set; } = new() { 1, 2, 3, 4, 5 };
    }

    public class SearchSuggestion
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
