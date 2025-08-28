using EcommercePWA.Services;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;
using EcommercePWA.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Registre os serviços ANTES de builder.Build()
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLLiteConnection"))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🛠️ Executa Seed/Migration e inicializa o banco
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var authService = services.GetRequiredService<IAuthService>();
    var productService = services.GetRequiredService<IProductService>();
    var couponService = services.GetRequiredService<ICouponService>();

    try
    {
        // Garantir que o banco existe e está atualizado
        await context.Database.EnsureCreatedAsync();
        
        // Inicializar usuário admin
        await authService.InitializeDatabaseAsync();
        
        // Inicializar produtos e categorias
        await productService.InitializeDatabaseAsync();
        
        // Inicializar cupons
        await couponService.InitializeDatabaseAsync();
        
        Console.WriteLine("✅ Banco de dados inicializado com sucesso!");
        Console.WriteLine("👤 Usuário admin: admin@kaboom.com / admin123");
        Console.WriteLine("📦 Produtos e categorias carregados");
        Console.WriteLine("🎫 Cupons carregados");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro ao inicializar banco de dados: {ex.Message}");
    }
}

// Configure o pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

// Add session middleware
app.UseSession();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kaboom E-commerce API v1"));

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
