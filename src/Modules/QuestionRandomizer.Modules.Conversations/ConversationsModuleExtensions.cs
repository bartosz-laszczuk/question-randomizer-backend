using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;
using QuestionRandomizer.Modules.Conversations.Infrastructure.Repositories;

namespace QuestionRandomizer.Modules.Conversations;

/// <summary>
/// Extension methods for registering the Conversations module services
/// </summary>
public static class ConversationsModuleExtensions
{
    /// <summary>
    /// Add Conversations module services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddConversationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
