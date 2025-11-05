namespace EcommercePWA.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        public List<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
    }
}
