using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using CinemaServer;
using CinemaServer.Components;
using CinemaServer.Models;
using CinemaServer.Services;
using CinemaServer.WebServices;

// ВАЖНО: Инициализация Npgsql ДОЛЖНА быть первой строкой!
NpgsqlConfig.Initialize();

var builder = WebApplication.CreateBuilder(args);

// ============================================
// FIXED PORT: 5103 (production fallback)
// ============================================
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://0.0.0.0:5103");
}

// ============================================
// DATABASE
// ============================================
builder.Services.AddDbContext<CinemaOnlineContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(3);
    });
});

// ============================================
// API SERVICES
// ============================================
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<RatingService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<ViewHistoryService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<GenreService>();
builder.Services.AddScoped<CollectionService>();

// ============================================
// BACKGROUND SERVICES
// ============================================
builder.Services.AddHostedService<SubscriptionCleanupService>();

// ============================================
// BLAZOR WEB (сайт на том же порту)
// ============================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient<CinemaApiService>();
builder.Services.AddScoped<AuthStateService>();

// ============================================
// CONTROLLERS & SWAGGER
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cinema Online API",
        Version = "v1",
        Description = "RESTful API для онлайн-кинотеатра с управлением фильмами, подписками и пользователями"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        In = ParameterLocation.Header,
        Description = "Введите токен в формате: Bearer_{userId}_{role}_{guid}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================
// CORS
// ============================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cinema Online API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Cinema Online API";
});

app.UseCors();

// Статические файлы с запретом агрессивного кэширования —
// браузер обязан перепроверять файлы при каждом запросе
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers.Pragma = "no-cache";
        ctx.Context.Response.Headers.Expires = "0";
    }
});

app.UseAntiforgery();
app.MapControllers();

// Blazor pages (сайт)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.Now }))
   .WithName("HealthCheck")
   .WithTags("Health");

app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CinemaServer");
    logger.LogInformation("===========================================");
    logger.LogInformation("  CINEMA ONLINE запущен!");
    foreach (var url in app.Urls)
    {
        // 0.0.0.0 — «слушать всё», но открывать в браузере нужно localhost
        var browsable = url.Replace("://0.0.0.0", "://localhost");
        logger.LogInformation("  Сайт:    {Url}", browsable);
        logger.LogInformation("  Swagger: {Url}/swagger", browsable);
    }
    logger.LogInformation("===========================================");
});

app.Run();
