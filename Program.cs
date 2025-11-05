using EcommercePWA.Services;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;
using EcommercePWA.Models;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

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

builder.Services.AddScoped<IProductPricingService, ProductPricingService>();
builder.Services.AddScoped<ICartCalculationService, CartCalculationService>();

builder.Services.AddHttpClient<IMelhorEnvioService, MelhorEnvioService>();

builder.Services.Configure<MercadoPagoSettings>(
    builder.Configuration.GetSection("MercadoPago"));

builder.Services.AddHttpClient<IMercadoPagoService, MercadoPagoService>(client =>
{
    client.BaseAddress = new Uri("https://api.mercadopago.com");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<IPixService, PixService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 minutos de timeout
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
        Description = @"API REST Completa - E-commerce Kaboom

VISÃO GERAL
API completa para gerenciamento de e-commerce de periféricos e peças de computador.

AUTENTICAÇÃO
Esta API usa autenticação baseada em sessão HTTP. Para usar endpoints protegidos:

1. Faça login através do endpoint POST /api/AuthApi/login
2. Use as credenciais padrão:
   - Email: admin@kaboom.com
   - Senha: admin123
3. A sessão será mantida automaticamente nos cookies

FUNCIONALIDADES PRINCIPAIS

Autenticação (AuthApi)
- Login e registro de usuários
- Gerenciamento de sessões
- Controle de acesso (admin/user)

Carrinho (CartApi)
- Adicionar/remover produtos
- Gerenciar quantidades
- Aplicar cupons de desconto
- Calcular frete

Produtos (ProductApi)
- Catálogo completo de produtos
- Busca e filtros avançados
- Categorias e subcategorias
- Variações de produtos

Pedidos (OrderApi)
- Criar e gerenciar pedidos
- Rastreamento de entregas
- Histórico de compras

Lista de Desejos (WishlistApi)
- Adicionar produtos favoritos
- Transferir para carrinho

TECNOLOGIAS
- Framework: ASP.NET Core 9.0
- Banco de Dados: SQLite + Entity Framework Core
- Pagamentos: Mercado Pago SDK
- Frete: Melhor Envio API
- Real-time: SignalR

AMBIENTES
- Desenvolvimento: http://localhost:5005
- Swagger UI: http://localhost:5005/swagger
- API Base: http://localhost:5005/api

CONTATO
- Email: admin@kaboom.com
- GitHub: https://github.com/tutuca07segundo/EcommercePWA",
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

    // Organizar endpoints por controller
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);

    // Incluir comentários XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Configuração de autenticação (apenas informativa)
    c.AddSecurityDefinition("Session", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"Autenticação baseada em sessão HTTP
        
Para autenticar:
1. Use o endpoint POST /api/AuthApi/login
2. Credenciais padrão: admin@kaboom.com / admin123
3. A sessão será mantida automaticamente via cookies
4. Não é necessário preencher o campo 'Value' abaixo - é apenas informativo",
        Name = "ASP.NET_SessionId",
        In = Microsoft.OpenApi.Models.ParameterLocation.Cookie,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Session"
    });
    
    // Não aplicar security requirement globalmente - apenas informativo
    // Os endpoints protegidos usam [RequireAuth] no backend
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                              Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var authService = services.GetRequiredService<IAuthService>();
    var productService = services.GetRequiredService<IProductService>();
    var couponService = services.GetRequiredService<ICouponService>();

    try
    {

        await context.Database.EnsureCreatedAsync();

        await authService.InitializeDatabaseAsync();

        await productService.InitializeDatabaseAsync();

        await couponService.InitializeDatabaseAsync();

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

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
