using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();

            if (!await _context.SubscriptionPlans.AnyAsync())
            {
                _logger.LogInformation("Seeding subscription plans...");

                var plans = new List<SubscriptionPlan>
                {
                    new SubscriptionPlan
                    {
                        Id = Guid.NewGuid(),
                        Name = "Free",
                        PlanType = PlanType.Free,
                        Price = 0,
                        MaxUsers = 3,
                        MaxProjects = 5,
                        MaxStorage = 1,
                        HasApiAccess = false,
                        HasPrioritySupport = false,
                        HasCustomBranding = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        FeaturesJson = "{\"features\": [\"Basic Features\", \"Email Support\"]}"
                    },
                    new SubscriptionPlan
                    {
                        Id = Guid.NewGuid(),
                        Name = "Pro",
                        PlanType = PlanType.Pro,
                        Price = 49.99m,
                        MaxUsers = 25,
                        MaxProjects = 50,
                        MaxStorage = 50,
                        HasApiAccess = true,
                        HasPrioritySupport = true,
                        HasCustomBranding = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        FeaturesJson = "{\"features\": [\"All Free Features\", \"API Access\", \"Priority Support\", \"Advanced Analytics\"]}"
                    },
                    new SubscriptionPlan
                    {
                        Id = Guid.NewGuid(),
                        Name = "Enterprise",
                        PlanType = PlanType.Enterprise,
                        Price = 199.99m,
                        MaxUsers = 999,
                        MaxProjects = 999,
                        MaxStorage = 500,
                        HasApiAccess = true,
                        HasPrioritySupport = true,
                        HasCustomBranding = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        FeaturesJson = "{\"features\": [\"All Pro Features\", \"Custom Branding\", \"Dedicated Support\", \"SLA\", \"Custom Integrations\"]}"
                    }
                };

                await _context.SubscriptionPlans.AddRangeAsync(plans);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription plans seeded successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}
