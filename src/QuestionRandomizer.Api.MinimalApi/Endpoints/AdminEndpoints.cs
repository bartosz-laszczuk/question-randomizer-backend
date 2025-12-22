namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using Microsoft.AspNetCore.Http.HttpResults;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Admin-only Minimal API endpoints for system management
/// </summary>
public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(AuthorizationPolicies.AdminPolicy);

        group.MapGet("/me", GetAdminInfo)
            .WithName("GetAdminInfo")
            .WithSummary("Get current admin user info (test endpoint)")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/health", GetSystemHealth)
            .WithName("GetSystemHealth")
            .WithSummary("Get system health metrics (admin only)")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/users", GetAllUsers)
            .WithName("GetAllUsers")
            .WithSummary("Get all users (future implementation)")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/analytics", GetSystemAnalytics)
            .WithName("GetSystemAnalytics")
            .WithSummary("Get system analytics (future implementation)")
            .Produces(StatusCodes.Status200OK);

        return group;
    }

    private static IResult GetAdminInfo(ICurrentUserService currentUserService)
    {
        var userId = currentUserService.GetUserId();
        var userEmail = currentUserService.GetUserEmail();
        var userRole = currentUserService.GetUserRole();

        return TypedResults.Ok(new
        {
            UserId = userId,
            Email = userEmail,
            Role = userRole,
            IsAdmin = currentUserService.IsAdmin(),
            Message = "You have admin access!"
        });
    }

    private static IResult GetSystemHealth()
    {
        // Future implementation: gather real system metrics
        return TypedResults.Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Message = "Admin health endpoint - full implementation pending"
        });
    }

    private static IResult GetAllUsers()
    {
        // Future: Implement GetAllUsersQuery
        return TypedResults.Ok(new
        {
            Message = "GetAllUsers endpoint - implementation pending",
            Note = "This will require Firebase Admin SDK integration to list all users"
        });
    }

    private static IResult GetSystemAnalytics()
    {
        // Future: Implement analytics aggregation
        return TypedResults.Ok(new
        {
            Message = "Analytics endpoint - implementation pending",
            Note = "This will aggregate statistics across all users"
        });
    }
}
