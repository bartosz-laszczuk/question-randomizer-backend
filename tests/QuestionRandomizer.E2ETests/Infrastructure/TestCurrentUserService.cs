namespace QuestionRandomizer.E2ETests.Infrastructure;

using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;

/// <summary>
/// Test implementation of ICurrentUserService that returns predefined test user information
/// </summary>
public class TestCurrentUserService : ICurrentUserService
{
    public string GetUserId()
    {
        return E2ETestWebApplicationFactory.TestUserId;
    }

    public string? GetUserEmail()
    {
        return E2ETestWebApplicationFactory.TestUserEmail;
    }

    public string GetUserRole()
    {
        // Default test user has standard User role
        return AuthorizationPolicies.UserRole;
    }

    public bool IsInRole(string role)
    {
        return GetUserRole().Equals(role, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsAdmin()
    {
        return false; // Test user is not admin by default
    }

    public bool IsPremiumUser()
    {
        return false; // Test user is not premium by default
    }
}
