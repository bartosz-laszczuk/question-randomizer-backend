namespace QuestionRandomizer.Modules.Agent;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Infrastructure.Services;

public static class AgentModuleExtensions
{
    public static IServiceCollection AddAgentModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IAgentService, AgentService>(client =>
        {
            var baseUrl = configuration["AgentService:BaseUrl"] ?? "http://localhost:3002";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(
                int.Parse(configuration["AgentService:TimeoutSeconds"] ?? "60"));
        })
        .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
