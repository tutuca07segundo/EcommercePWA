using EcommercePWA.Services;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;
using EcommercePWA.Models;
using EcommercePWA.Constants;
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

// Registrar serviços principais
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<ICategoryPromotionService, CategoryPromotionService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<DataSeederService>();

// Registrar novos serviços de Clean Code
builder.Services.AddScoped<IProductPricingService, ProductPricingService>();
builder.Services.AddScoped<ICartCalculationService, CartCalculationService>();

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
    options.IdleTimeout = TimeSpan.FromMinutes(BusinessConstants.SESSION_TIMEOUT_MINUTES);
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
        Version = "v2.0",
        Description = @"API REST completa para o e-commerce Kaboom - Periféricos e Peças de Computador
        
        **Funcionalidades Principais:**
        - 🔐 Sistema de autenticação completo
        - 🛒 Gerenciamento de carrinho de compras
        - 📦 Catálogo de produtos com filtros avançados
        - 💳 Integração com Mercado Pago
        - 🤖 Chatbot inteligente
        - 📊 Sistema de pedidos e rastreamento
        - 🎯 Sistema de cupons e promoções
        - ❤️ Lista de desejos
        - ⭐ Sistema de avaliações
        
        **Tecnologias:**
        - ASP.NET Core 9.0
        - Entity Framework Core
        - SQLite Database
        - SignalR para tempo real
        - Mercado Pago SDK
        
        **Ambientes:**
        - Desenvolvimento: http://localhost:5005
        - Swagger UI: http://localhost:5005/swagger",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Kaboom Development Team",
            Email = "admin@kaboom.com",
            Url = new Uri("https://github.com/tutuca07segundo/EcommercePWA")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    
    // Configurar tags para organização
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);
    
    // Incluir comentários XML para documentação
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Configurar esquemas de segurança
    c.AddSecurityDefinition("Session", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Autenticação baseada em sessão. Faça login primeiro através do endpoint /api/AuthApi/login",
        Name = "Session",
        In = Microsoft.OpenApi.Models.ParameterLocation.Cookie,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Session"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Session"
                }
            },
            new string[] {}
        }
    });
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
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kaboom E-commerce API v2.0");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Kaboom E-commerce API Documentation";
    c.DefaultModelsExpandDepth(-1);
    c.DisplayRequestDuration();
    c.EnableValidator();
    c.SupportedSubmitMethods(new[] { 
        Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get, 
        Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post, 
        Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put, 
        Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete 
    });
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
