namespace QuestionRandomizer.Api.Controllers.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.Infrastructure.Authorization;
using QuestionRandomizer.Application.Interfaces;

/// <summary>
/// Admin-only controller for system management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.AdminPolicy)]
public class AdminController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ICurrentUserService currentUserService,
        ILogger<AdminController> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get current admin user info (test endpoint)
    /// </summary>
    /// <returns>Admin user information</returns>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAdminInfo()
    {
        var userId = _currentUserService.GetUserId();
        var userEmail = _currentUserService.GetUserEmail();
        var userRole = _currentUserService.GetUserRole();

        return Ok(new
        {
            UserId = userId,
            Email = userEmail,
            Role = userRole,
            IsAdmin = _currentUserService.IsAdmin(),
            Message = "You have admin access!"
        });
    }

    /// <summary>
    /// Get system health metrics (admin only)
    /// </summary>
    /// <returns>System health information</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSystemHealth()
    {
        // Future implementation: gather real system metrics
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Message = "Admin health endpoint - full implementation pending"
        });
    }

    /// <summary>
    /// Placeholder for getting all users (future implementation)
    /// </summary>
    /// <returns>Placeholder response</returns>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllUsers()
    {
        // Future: Implement GetAllUsersQuery
        return Ok(new
        {
            Message = "GetAllUsers endpoint - implementation pending",
            Note = "This will require Firebase Admin SDK integration to list all users"
        });
    }

    /// <summary>
    /// Placeholder for system analytics (future implementation)
    /// </summary>
    /// <returns>Placeholder response</returns>
    [HttpGet("analytics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSystemAnalytics()
    {
        // Future: Implement analytics aggregation
        return Ok(new
        {
            Message = "Analytics endpoint - implementation pending",
            Note = "This will aggregate statistics across all users"
        });
    }
}
