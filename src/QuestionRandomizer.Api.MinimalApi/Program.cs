using QuestionRandomizer.Application;
using QuestionRandomizer.Infrastructure;
using QuestionRandomizer.Infrastructure.Authorization;
using QuestionRandomizer.SharedKernel;
using QuestionRandomizer.Modules.Questions;
using QuestionRandomizer.Modules.Conversations;
using QuestionRandomizer.Modules.Randomization;
using QuestionRandomizer.Modules.Agent;
using QuestionRandomizer.Api.MinimalApi.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Skip Swagger in Testing environment to avoid version conflicts
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSwaggerGen();
}

// Add built-in ProblemDetails support for Minimal APIs
builder.Services.AddProblemDetails();

// Add SharedKernel (cross-cutting concerns, domain events)
builder.Services.AddSharedKernel(builder.Configuration, builder.Environment);

// Add Modules (modular monolith architecture)
builder.Services.AddQuestionsModule(builder.Configuration);
builder.Services.AddConversationsModule(builder.Configuration);
builder.Services.AddRandomizationModule();
builder.Services.AddAgentModule(builder.Configuration);

// Add Application layer (MediatR, FluentValidation) - LEGACY, will be removed
builder.Services.AddApplication();

// Add Infrastructure layer (Firebase, Repositories) - LEGACY, will be removed
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? Array.Empty<string>();
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("QuestionRandomizer.Api.MinimalApi"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

builder.Logging.AddOpenTelemetry(options =>
{
    options.AddConsoleExporter();
});

// Add Health Checks
builder.Services.AddHealthChecks();

// Add Authentication & Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var projectId = builder.Configuration["Firebase:ProjectId"];
    options.Authority = $"https://securetoken.google.com/{projectId}";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = $"https://securetoken.google.com/{projectId}",
        ValidateAudience = true,
        ValidAudience = projectId,
        ValidateLifetime = true
    };
});

// Add Authorization with Policies
builder.Services.AddAuthorization(options =>
{
    // User Policy - Basic authenticated user (default)
    options.AddPolicy(AuthorizationPolicies.UserPolicy, policy =>
        policy.RequireAuthenticatedUser());

    // Premium User Policy - Premium tier users and admins
    options.AddPolicy(AuthorizationPolicies.PremiumUserPolicy, policy =>
        policy.RequireClaim("role",
            AuthorizationPolicies.PremiumUserRole,
            AuthorizationPolicies.AdminRole)); // Admin has all premium features

    // Admin Policy - Platform administrator only
    options.AddPolicy(AuthorizationPolicies.AdminPolicy, policy =>
        policy.RequireClaim("role", AuthorizationPolicies.AdminRole));

    // Feature-specific policies
    options.AddPolicy("CanUseAdvancedAI", policy =>
        policy.RequireClaim("role",
            AuthorizationPolicies.PremiumUserRole,
            AuthorizationPolicies.AdminRole));

    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireClaim("role", AuthorizationPolicies.AdminRole));

    options.AddPolicy("CanViewAllQuestions", policy =>
        policy.RequireClaim("role", AuthorizationPolicies.AdminRole));
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Question Randomizer Minimal API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapQuestionEndpoints();
app.MapCategoryEndpoints();
app.MapQualificationEndpoints();
app.MapConversationEndpoints();
app.MapRandomizationEndpoints();
app.MapAgentEndpoints();
app.MapHealthChecks("/health");

app.Run();

// Make Program class accessible to tests
public partial class Program { }
