using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.BackgroundServices;
using SaaS.Infrastructure.Caching;
using SaaS.Infrastructure.Persistence;
using SaaS.Infrastructure.Persistence.Repositories;
using SaaS.Infrastructure.Services;
using StackExchange.Redis;
using System.Text;

namespace SaaS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Services
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IAuditService, AuditService>();

        // Memory Cache (required for InMemoryCacheService fallback)
        services.AddMemoryCache();

        // Redis
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnection));
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }

        // Background Services
        services.AddHostedService<SubscriptionExpiryBackgroundService>();

        // Database Seeder
        services.AddScoped<DatabaseSeeder>();

        // JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"));
            options.AddPolicy("ManagerOrAbove", policy => policy.RequireRole("Manager", "Admin", "SuperAdmin"));
        });

        return services;
    }
}
