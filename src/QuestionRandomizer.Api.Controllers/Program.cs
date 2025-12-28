using QuestionRandomizer.SharedKernel;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.Modules.Questions;
using QuestionRandomizer.Modules.Conversations;
using QuestionRandomizer.Modules.Randomization;
using QuestionRandomizer.Modules.Agent;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add SignalR for real-time streaming
builder.Services.AddSignalR();

// Only add Swagger in Development environment (not Testing)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSwaggerGen();
}

// Add ProblemDetails
builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();

    // Map domain exceptions to proper HTTP status codes
    options.MapToStatusCode<QuestionRandomizer.SharedKernel.Domain.Exceptions.NotFoundException>(StatusCodes.Status404NotFound);
    options.MapToStatusCode<QuestionRandomizer.SharedKernel.Domain.Exceptions.ValidationException>(StatusCodes.Status400BadRequest);
    options.MapToStatusCode<QuestionRandomizer.SharedKernel.Domain.Exceptions.UnauthorizedException>(StatusCodes.Status401Unauthorized);
});

// Add SharedKernel (cross-cutting concerns, domain events)
builder.Services.AddSharedKernel(builder.Configuration, builder.Environment);

// Add Modules (modular monolith architecture)
builder.Services.AddQuestionsModule(builder.Configuration);
builder.Services.AddConversationsModule(builder.Configuration);
builder.Services.AddRandomizationModule();
builder.Services.AddAgentModule(builder.Configuration);

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

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("QuestionRandomizer.Api.Controllers"))
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

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseProblemDetails();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Question Randomizer API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Map SignalR Hub
app.MapHub<QuestionRandomizer.Modules.Agent.Infrastructure.Hubs.AgentHub>("/agentHub");

app.Run();

// Make Program class accessible to tests
public partial class Program { }
