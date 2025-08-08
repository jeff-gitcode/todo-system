using Microsoft.Extensions.DependencyInjection;
using TodoSystem.Application.Dtos;
using AutoMapper;
using System.Reflection;
using TodoSystem.Application.Behaviors;
using MediatR;
using FluentValidation;

namespace TodoSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services here, for example:
        // services.AddScoped<IMyService, MyService>();

        // Use this for older AutoMapper versions
        services.AddAutoMapper(config =>
        {
            config.AddMaps(typeof(DependencyInjection).Assembly);
            // Or explicitly add profiles:
            // config.AddProfile<TodoProfile>();
        });

        // Register all validators in assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register MediatR validation pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
