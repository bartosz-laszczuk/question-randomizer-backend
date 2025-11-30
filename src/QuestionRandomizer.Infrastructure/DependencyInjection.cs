namespace QuestionRandomizer.Infrastructure;

using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;
using QuestionRandomizer.Infrastructure.Firebase;
using QuestionRandomizer.Infrastructure.Repositories;
using QuestionRandomizer.Infrastructure.Services;

/// <summary>
/// Dependency injection configuration for the Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">The hosting environment (optional)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        // Configure Firebase (skip in Testing environment)
        var isTestEnvironment = environment?.IsEnvironment("Testing") ?? false;
        if (!isTestEnvironment)
        {
            services.AddFirebase(configuration);
        }
        else
        {
            // Register a null FirestoreDb for testing (will be mocked anyway)
            services.AddSingleton<FirestoreDb>(provider => null!);
        }

        // Register repositories
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IQualificationRepository, QualificationRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IRandomizationRepository, RandomizationRepository>();

        // Register nested resource repositories (subcollections)
        services.AddScoped<ISelectedCategoryRepository, SelectedCategoryRepository>();
        services.AddScoped<IUsedQuestionRepository, UsedQuestionRepository>();
        services.AddScoped<IPostponedQuestionRepository, PostponedQuestionRepository>();

        // Register services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register Agent Service with HttpClient and Polly retry policies
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

    private static IServiceCollection AddFirebase(this IServiceCollection services, IConfiguration configuration)
    {
        // Skip Firebase initialization in Testing environment
        var skipFirebase = configuration.GetValue<bool>("Testing:SkipFirebase");
        if (skipFirebase)
        {
            // Register a null FirestoreDb for testing (will be mocked anyway)
            services.AddSingleton<FirestoreDb>(provider => null!);
            return services;
        }

        var firebaseSettings = configuration.GetSection("Firebase").Get<FirebaseSettings>();

        if (firebaseSettings == null || string.IsNullOrEmpty(firebaseSettings.ProjectId))
        {
            throw new InvalidOperationException("Firebase configuration is missing or invalid");
        }

        // Initialize Firebase Admin SDK
        if (FirebaseApp.DefaultInstance == null)
        {
            if (!string.IsNullOrEmpty(firebaseSettings.CredentialsPath) && File.Exists(firebaseSettings.CredentialsPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebaseSettings.CredentialsPath);
            }

            FirebaseApp.Create(new AppOptions
            {
                ProjectId = firebaseSettings.ProjectId,
                Credential = string.IsNullOrEmpty(firebaseSettings.CredentialsPath)
                    ? GoogleCredential.GetApplicationDefault()
                    : GoogleCredential.FromFile(firebaseSettings.CredentialsPath)
            });
        }

        // Register FirestoreDb
        services.AddSingleton(provider =>
        {
            return FirestoreDb.Create(firebaseSettings.ProjectId);
        });

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
