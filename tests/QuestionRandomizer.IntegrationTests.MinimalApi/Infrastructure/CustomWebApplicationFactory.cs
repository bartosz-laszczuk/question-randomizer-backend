namespace QuestionRandomizer.IntegrationTests.MinimalApi.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Custom WebApplicationFactory for Minimal API integration tests with mocked dependencies
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IQuestionRepository> QuestionRepositoryMock { get; } = new();
    public Mock<ICategoryRepository> CategoryRepositoryMock { get; } = new();
    public Mock<IQualificationRepository> QualificationRepositoryMock { get; } = new();
    public Mock<IConversationRepository> ConversationRepositoryMock { get; } = new();
    public Mock<IMessageRepository> MessageRepositoryMock { get; } = new();
    public Mock<IRandomizationRepository> RandomizationRepositoryMock { get; } = new();
    public Mock<IAgentService> AgentServiceMock { get; } = new();
    public Mock<ICurrentUserService> CurrentUserServiceMock { get; } = new();

    public const string TestUserId = "test-user-123";
    public const string TestUserEmail = "test@example.com";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set test environment
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Remove existing configuration sources
            config.Sources.Clear();

            // Add in-memory configuration for tests
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Testing:SkipFirebase"] = "true",
                ["Firebase:ProjectId"] = "test-project",
                ["Firebase:CredentialsPath"] = "test-credentials.json",
                ["AgentService:BaseUrl"] = "http://localhost:3002",
                ["AgentService:TimeoutSeconds"] = "60",
                ["Cors:AllowedOrigins:0"] = "http://localhost:4200"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing repository registrations
            RemoveService<IQuestionRepository>(services);
            RemoveService<ICategoryRepository>(services);
            RemoveService<IQualificationRepository>(services);
            RemoveService<IConversationRepository>(services);
            RemoveService<IMessageRepository>(services);
            RemoveService<IRandomizationRepository>(services);
            RemoveService<IAgentService>(services);
            RemoveService<ICurrentUserService>(services);

            // Disable authentication for integration tests
            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // Setup default mock behaviors
            CurrentUserServiceMock.Setup(x => x.GetUserId()).Returns(TestUserId);
            CurrentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(TestUserEmail);

            // Register mocked services
            services.AddSingleton(QuestionRepositoryMock.Object);
            services.AddSingleton(CategoryRepositoryMock.Object);
            services.AddSingleton(QualificationRepositoryMock.Object);
            services.AddSingleton(ConversationRepositoryMock.Object);
            services.AddSingleton(MessageRepositoryMock.Object);
            services.AddSingleton(RandomizationRepositoryMock.Object);
            services.AddSingleton(AgentServiceMock.Object);
            services.AddSingleton(CurrentUserServiceMock.Object);
        });
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    public void ResetMocks()
    {
        QuestionRepositoryMock.Reset();
        CategoryRepositoryMock.Reset();
        QualificationRepositoryMock.Reset();
        ConversationRepositoryMock.Reset();
        MessageRepositoryMock.Reset();
        RandomizationRepositoryMock.Reset();
        AgentServiceMock.Reset();
        CurrentUserServiceMock.Reset();

        // Re-setup default behaviors
        CurrentUserServiceMock.Setup(x => x.GetUserId()).Returns(TestUserId);
        CurrentUserServiceMock.Setup(x => x.GetUserEmail()).Returns(TestUserEmail);
    }
}
