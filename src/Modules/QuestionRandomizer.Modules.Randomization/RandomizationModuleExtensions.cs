using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.Modules.Randomization.Infrastructure.Repositories;

namespace QuestionRandomizer.Modules.Randomization;

/// <summary>
/// Dependency Injection extension methods for the Randomization module
/// Demonstrates modular monolith architecture with cross-module event subscription
/// </summary>
public static class RandomizationModuleExtensions
{
    /// <summary>
    /// Register all Randomization module services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The modified service collection</returns>
    public static IServiceCollection AddRandomizationModule(this IServiceCollection services)
    {
        // Register domain repositories
        services.AddScoped<IRandomizationRepository, RandomizationRepository>();
        services.AddScoped<ISelectedCategoryRepository, SelectedCategoryRepository>();
        services.AddScoped<IUsedQuestionRepository, UsedQuestionRepository>();
        services.AddScoped<IPostponedQuestionRepository, PostponedQuestionRepository>();

        // Register MediatR handlers and event handlers from this assembly
        // This will auto-discover all IRequestHandler<,> and INotificationHandler<> implementations
        // including CategoryDeletedEventHandler (cross-module event subscription)
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register validators from this assembly (if any exist in the future)
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
