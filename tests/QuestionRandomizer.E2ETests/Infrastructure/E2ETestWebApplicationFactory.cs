namespace QuestionRandomizer.E2ETests.Infrastructure;

using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Custom WebApplicationFactory for E2E tests
/// Uses real Firestore repositories but configured for testing environment
/// </summary>
public class E2ETestWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestUserId = "e2e-test-user-123";
    public const string TestUserEmail = "e2e-test@example.com";
    public const string TestProjectId = "e2e-test-project";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set test environment
        builder.UseEnvironment("E2ETesting");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Remove existing configuration sources
            config.Sources.Clear();

            // Add in-memory configuration for E2E tests
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Testing:SkipFirebase"] = "true", // Skip Firebase initialization
                ["Firebase:ProjectId"] = TestProjectId,
                ["Firebase:CredentialsPath"] = "",
                ["AgentService:BaseUrl"] = "http://localhost:3002",
                ["AgentService:TimeoutSeconds"] = "60",
                ["Cors:AllowedOrigins:0"] = "http://localhost:4200"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing FirestoreDb registration
            var firestoreDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(FirestoreDb));
            if (firestoreDescriptor != null)
            {
                services.Remove(firestoreDescriptor);
            }

            // Register null FirestoreDb (won't be used in E2E tests as we use real repositories with in-memory storage)
            services.AddSingleton<FirestoreDb>(provider => null!);

            // Remove Swagger-related application parts to prevent assembly loading issues
            var partManager = services
                .FirstOrDefault(d => d.ServiceType == typeof(Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager))
                ?.ImplementationInstance as Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager;

            if (partManager != null)
            {
                var swaggerParts = partManager.ApplicationParts
                    .Where(p => p.Name.Contains("Swashbuckle", StringComparison.OrdinalIgnoreCase) ||
                                p.Name.Contains("OpenApi", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var part in swaggerParts)
                {
                    partManager.ApplicationParts.Remove(part);
                }
            }

            // Disable authentication for E2E tests
            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // Override CurrentUserService to return test user
            var currentUserServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICurrentUserService));
            if (currentUserServiceDescriptor != null)
            {
                services.Remove(currentUserServiceDescriptor);
            }
            services.AddScoped<ICurrentUserService, TestCurrentUserService>();

            // Note: Repositories remain as real implementations, but they work with in-memory Firestore
            // This provides a more realistic E2E testing scenario than pure mocks
        });
    }
}
