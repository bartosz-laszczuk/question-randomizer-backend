using QuestionRandomizer.Application;
using QuestionRandomizer.Infrastructure;
using QuestionRandomizer.Api.MinimalApi.Endpoints;
using Hellang.Middleware.ProblemDetails;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add ProblemDetails
builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();

    // Map domain exceptions to proper HTTP status codes
    options.MapToStatusCode<QuestionRandomizer.Domain.Exceptions.NotFoundException>(StatusCodes.Status404NotFound);
    options.MapToStatusCode<QuestionRandomizer.Domain.Exceptions.ValidationException>(StatusCodes.Status400BadRequest);
    options.MapToStatusCode<QuestionRandomizer.Domain.Exceptions.UnauthorizedException>(StatusCodes.Status401Unauthorized);
});

// Add Application layer (MediatR, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure layer (Firebase, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

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

// Add Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseProblemDetails();

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
app.MapRandomizationEndpoints();
app.MapConversationEndpoints();
app.MapHealthChecks("/health");

app.Run();

// Make Program class accessible to tests
public partial class Program { }
