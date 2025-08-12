namespace EcommercePWA.Models
{
    public class ProductViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedCategory { get; set; } = string.Empty;
        public string SortOrder { get; set; } = "name"; // name, price_asc, price_desc
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
