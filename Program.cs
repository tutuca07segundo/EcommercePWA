using EcommercePWA.Services;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;
using EcommercePWA.Models;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ✅ Registre os serviços ANTES de builder.Build()
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Registrar serviços
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
// builder.Services.AddScoped<IPromotionService, PromotionService>(); // Comentado temporariamente
builder.Services.AddScoped<ICategoryPromotionService, CategoryPromotionService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<DataSeederService>();

// Add Melhor Envio service
builder.Services.AddHttpClient<IMelhorEnvioService, MelhorEnvioService>();

// Configure MercadoPago settings
builder.Services.Configure<MercadoPagoSettings>(
    builder.Configuration.GetSection("MercadoPago"));

// Add HttpClient for MercadoPago service with proper configuration
builder.Services.AddHttpClient<IMercadoPagoService, MercadoPagoService>(client =>
{
    client.BaseAddress = new Uri("https://api.mercadopago.com");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Kaboom E-commerce API",
        Version = "v1",
        Description = "API REST completa para o e-commerce Kaboom - Periféricos e Peças de Computador",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Kaboom Team",
            Email = "admin@kaboom.com"
        }
    });
    
    // Incluir comentários XML para documentação
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configuração para suportar proxies reversos
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                              Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

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
        
        // Inicializar subcategorias
        var dataSeeder = services.GetRequiredService<DataSeederService>();
        await dataSeeder.SeedSubcategoriesAsync();
        
        Console.WriteLine("✅ Banco de dados inicializado com sucesso!");
        Console.WriteLine("👤 Usuário admin: admin@kaboom.com / admin123");
        Console.WriteLine("📦 Produtos e categorias carregados");
        Console.WriteLine("🎫 Cupons carregados");
        Console.WriteLine("🏷️ Subcategorias carregadas");
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
