using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;
using QuestionRandomizer.Modules.Questions.Infrastructure.Repositories;

namespace QuestionRandomizer.Modules.Questions;

public static class QuestionsModuleExtensions
{
    public static IServiceCollection AddQuestionsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IQualificationRepository, QualificationRepository>();

        // Register MediatR handlers and validators from this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
