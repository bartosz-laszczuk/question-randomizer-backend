namespace QuestionRandomizer.Modules.Agent;

using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Application.Tools;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Agent.Application.Tools.DataAnalysis;
using QuestionRandomizer.Modules.Agent.Application.Tools.DataModification;
using QuestionRandomizer.Modules.Agent.Application.Tools.DataRetrieval;
using QuestionRandomizer.Modules.Agent.Infrastructure.AI;
using QuestionRandomizer.Modules.Agent.Infrastructure.Queue;

/// <summary>
/// Extension methods for registering the Agent module services
/// </summary>
public static class AgentModuleExtensions
{
    public static IServiceCollection AddAgentModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure agent settings
        services.Configure<AgentConfiguration>(options =>
        {
            options.ApiKey = configuration["Anthropic:ApiKey"] ?? string.Empty;
            options.Model = configuration["Anthropic:Model"] ?? "claude-sonnet-4-5-20250929";
            options.MaxIterations = int.Parse(configuration["Anthropic:MaxIterations"] ?? "20");
            options.Temperature = double.Parse(configuration["Anthropic:Temperature"] ?? "0");
            options.MaxTokens = int.Parse(configuration["Anthropic:MaxTokens"] ?? "4096");
            options.TimeoutSeconds = int.Parse(configuration["Anthropic:TimeoutSeconds"] ?? "120");
        });

        // Register core agent services
        services.AddScoped<IAgentExecutor, AgentExecutor>();
        services.AddScoped<IAgentService, Infrastructure.Services.AgentService>();
        services.AddScoped<IAgentTaskRepository, Infrastructure.Repositories.AgentTaskRepository>();

        // Register all 15 agent tools
        // Data Retrieval Tools (6)
        services.AddScoped<IAgentTool, GetQuestionsTool>();
        services.AddScoped<IAgentTool, GetQuestionByIdTool>();
        services.AddScoped<IAgentTool, GetCategoriesTool>();
        services.AddScoped<IAgentTool, GetQualificationsTool>();
        services.AddScoped<IAgentTool, GetUncategorizedQuestionsTool>();
        services.AddScoped<IAgentTool, SearchQuestionsTool>();

        // Data Modification Tools (7)
        services.AddScoped<IAgentTool, CreateQuestionTool>();
        services.AddScoped<IAgentTool, UpdateQuestionTool>();
        services.AddScoped<IAgentTool, DeleteQuestionTool>();
        services.AddScoped<IAgentTool, UpdateQuestionCategoryTool>();
        services.AddScoped<IAgentTool, CreateCategoryTool>();
        services.AddScoped<IAgentTool, CreateQualificationTool>();
        services.AddScoped<IAgentTool, BatchUpdateQuestionsTool>();

        // Data Analysis Tools (2)
        services.AddScoped<IAgentTool, FindDuplicateQuestionsTool>();
        services.AddScoped<IAgentTool, AnalyzeQuestionDifficultyTool>();

        // Register tool registry
        services.AddScoped<ToolRegistry>();

        // Register background job services
        services.AddScoped<ITaskQueueService, TaskQueueService>();
        services.AddScoped<AgentTaskProcessor>();

        // Configure Hangfire for background job processing
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage());

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = int.Parse(configuration["Hangfire:WorkerCount"] ?? "2");
        });

        return services;
    }
}
