using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Application.Behaviors;
using System.Reflection;

namespace SaaS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        services.AddValidatorsFromAssembly(assembly);
        
        services.AddAutoMapper(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
