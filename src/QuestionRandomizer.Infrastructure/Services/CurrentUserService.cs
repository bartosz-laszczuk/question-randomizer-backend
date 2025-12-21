namespace QuestionRandomizer.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Infrastructure.Authorization;
using System.Security.Claims;

/// <summary>
/// Service for accessing the current authenticated user from HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        // Firebase uses both standard claims and custom "user_id" claim
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        return userId;
    }

    public string? GetUserEmail()
    {
        // Firebase uses both standard claims and custom "email" claim
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
    }

    public string GetUserRole()
    {
        // Firebase custom claims use "role" key
        var role = _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;

        // Default to "User" role if no role claim exists
        return role ?? AuthorizationPolicies.UserRole;
    }

    public bool IsInRole(string role)
    {
        var userRole = GetUserRole();
        return userRole.Equals(role, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsAdmin()
    {
        return IsInRole(AuthorizationPolicies.AdminRole);
    }

    public bool IsPremiumUser()
    {
        // Premium users have PremiumUser role, and admins have all premium features
        return IsInRole(AuthorizationPolicies.PremiumUserRole) || IsAdmin();
    }
}
