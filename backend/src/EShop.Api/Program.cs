using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using EShop.Infrastructure.Persistence;
using EShop.Infrastructure.Persistence.Repositories;
using EShop.Infrastructure.Security;
using EShop.Infrastructure.Caching;
using EShop.Domain.Customers;
using EShop.Domain.Addresses;
using EShop.Domain.Products;
using EShop.Domain.Orders;
using EShop.Domain.Auth;
using EShop.Domain.Common;
using EShop.Application.Auth;
using EShop.Application.Products;
using EShop.Application.Orders;
using EShop.Application.Customers;
using EShop.Application.Addresses;
using EShop.Application.ActivityLogs;
using EShop.Application.Common;
using EShop.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=./data/app.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// unit of work and domain events
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

// repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

// domain event handlers
builder.Services.AddScoped<IDomainEventHandler<OrderPlaced>, EShop.Application.Orders.EventHandlers.OrderPlacedEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<OrderStatusChanged>, EShop.Application.Orders.EventHandlers.OrderStatusChangedEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<UserLoggedIn>, EShop.Application.Auth.EventHandlers.UserLoggedInEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<UserLoggedOut>, EShop.Application.Auth.EventHandlers.UserLoggedOutEventHandler>();

// caching
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICache, MemoryCacheAdapter>();
builder.Services.AddScoped<ICacheInvalidator, CacheInvalidator>();
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));

// services
builder.Services.AddScoped<DataImportService>();

// jwt
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<JwtTokenService>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? new JwtSettings();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes)
        };
    });

builder.Services.AddAuthorization();

// handlers
builder.Services.AddScoped<RegisterCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<RefreshTokenCommandHandler>();
builder.Services.AddScoped<LogoutCommandHandler>();
builder.Services.AddScoped<SearchProductsQueryHandler>();
builder.Services.AddScoped<GetProductByIdQueryHandler>();
builder.Services.AddScoped<CheckoutOrderCommandHandler>();
builder.Services.AddScoped<GetCustomerOrdersQueryHandler>();
builder.Services.AddScoped<GetOrderByIdQueryHandler>();
builder.Services.AddScoped<GetOrdersQueryHandler>();
builder.Services.AddScoped<UpdateOrderStatusCommandHandler>();
builder.Services.AddScoped<GetCustomerByIdQueryHandler>();
builder.Services.AddScoped<UpdateCustomerCommandHandler>();
builder.Services.AddScoped<GetCustomerAddressesQueryHandler>();
builder.Services.AddScoped<CreateAddressCommandHandler>();
builder.Services.AddScoped<SetDefaultAddressCommandHandler>();
builder.Services.AddScoped<DeleteAddressCommandHandler>();
builder.Services.AddScoped<GetActivityLogsQueryHandler>();

// data import on startup
builder.Services.AddHostedService<DataImportHostedService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EShop API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ensure database created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Hosted service that triggers data import on application startup.
/// </summary>
public class DataImportHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataImportHostedService> _logger;

    public DataImportHostedService(IServiceProvider serviceProvider, ILogger<DataImportHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Checking for data import...");

        using var scope = _serviceProvider.CreateScope();
        var importer = scope.ServiceProvider.GetRequiredService<DataImportService>();

        await importer.ImportIfNeededAsync(ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
