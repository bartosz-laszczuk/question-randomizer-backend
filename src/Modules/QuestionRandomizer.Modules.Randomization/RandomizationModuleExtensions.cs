using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QuestionRandomizer.Modules.Randomization.Application.EventHandlers;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.Modules.Randomization.Infrastructure.Repositories;
using QuestionRandomizer.Modules.Questions.Domain.Events;

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

        // Register event handler - subscribes to CategoryDeletedEvent from Questions module
        services.AddScoped<INotificationHandler<CategoryDeletedEvent>, CategoryDeletedEventHandler>();

        return services;
    }
}
