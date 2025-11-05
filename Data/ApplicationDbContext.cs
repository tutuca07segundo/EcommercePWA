using Microsoft.EntityFrameworkCore;
using EcommercePWA.Models;

namespace EcommercePWA.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<ProductVariation> ProductVariations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<ProductDiscount> ProductDiscounts { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<CategoryPromotion> CategoryPromotions { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.Weight)
                .HasColumnType("decimal(8,2)");

            modelBuilder.Entity<User>()
                .Property(u => u.Password)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(150);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.Total)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TaxAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MinimumOrderAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductVariation>()
                .Property(pv => pv.PriceModifier)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Review>()
                .Property(r => r.Rating)
                .HasColumnType("decimal(3,1)");

            modelBuilder.Entity<PasswordResetRequest>()
                .Property(p => p.Email)
                .HasMaxLength(150);
            modelBuilder.Entity<PasswordResetRequest>()
                .Property(p => p.Code)
                .HasMaxLength(6);

            modelBuilder.Entity<Subcategory>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Discount>()
                .Property(d => d.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Discount>()
                .Property(d => d.DiscountPercentage)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<ProductDiscount>()
                .HasKey(pd => new { pd.ProductId, pd.DiscountId });

            modelBuilder.Entity<ProductDiscount>()
                .HasOne(pd => pd.Product)
                .WithMany(p => p.ProductDiscounts)
                .HasForeignKey(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductDiscount>()
                .HasOne(pd => pd.Discount)
                .WithMany(d => d.ProductDiscounts)
                .HasForeignKey(pd => pd.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategoryPromotion>()
                .Property(cp => cp.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CategoryPromotion>()
                .Property(cp => cp.DiscountPercentage)
                .HasColumnType("decimal(5,2)");
        }
    }
}