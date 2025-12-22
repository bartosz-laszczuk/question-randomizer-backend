namespace QuestionRandomizer.SharedKernel;

using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestionRandomizer.SharedKernel.Application.Behaviors;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.SharedKernel.Infrastructure.Firebase;
using QuestionRandomizer.SharedKernel.Infrastructure.Services;

/// <summary>
/// Dependency injection configuration for the SharedKernel
/// </summary>
public static class SharedKernelExtensions
{
    /// <summary>
    /// Adds SharedKernel services to the dependency injection container
    /// Includes Firebase, MediatR behaviors, and cross-cutting services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">The hosting environment (optional)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSharedKernel(
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

        // Register MediatR pipeline behaviors (global)
        // Order matters: Logging first, then Validation
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register authentication/authorization services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUserManagementService, UserManagementService>();

        return services;
    }

    private static IServiceCollection AddFirebase(
        this IServiceCollection services,
        IConfiguration configuration)
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
            if (!string.IsNullOrEmpty(firebaseSettings.CredentialsPath) &&
                File.Exists(firebaseSettings.CredentialsPath))
            {
                Environment.SetEnvironmentVariable(
                    "GOOGLE_APPLICATION_CREDENTIALS",
                    firebaseSettings.CredentialsPath);
            }

            FirebaseApp.Create(new AppOptions
            {
                ProjectId = firebaseSettings.ProjectId,
                Credential = string.IsNullOrEmpty(firebaseSettings.CredentialsPath)
                    ? GoogleCredential.GetApplicationDefault()
                    : GoogleCredential.FromFile(firebaseSettings.CredentialsPath)
            });
        }

        // Register FirestoreDb singleton
        services.AddSingleton(provider => FirestoreDb.Create(firebaseSettings.ProjectId));

        return services;
    }
}
