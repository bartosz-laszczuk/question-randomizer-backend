namespace QuestionRandomizer.Infrastructure.Services;

using FirebaseAdmin.Auth;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Infrastructure.Authorization;

/// <summary>
/// Service for managing user roles and claims via Firebase Admin SDK
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Sets a role for a user via Firebase custom claims
    /// </summary>
    /// <param name="userId">Firebase user ID</param>
    /// <param name="role">Role to assign (User, PremiumUser, Admin)</param>
    Task SetUserRoleAsync(string userId, string role);

    /// <summary>
    /// Makes a user an admin
    /// </summary>
    /// <param name="userId">Firebase user ID</param>
    Task MakeUserAdminAsync(string userId);

    /// <summary>
    /// Upgrades a user to premium tier
    /// </summary>
    /// <param name="userId">Firebase user ID</param>
    Task UpgradeToPremiumAsync(string userId);

    /// <summary>
    /// Downgrades a premium user to regular user
    /// </summary>
    /// <param name="userId">Firebase user ID</param>
    Task DowngradeToUserAsync(string userId);

    /// <summary>
    /// Gets the custom claims for a user
    /// </summary>
    /// <param name="userId">Firebase user ID</param>
    /// <returns>Dictionary of custom claims</returns>
    Task<IDictionary<string, object>> GetUserClaimsAsync(string userId);
}

/// <summary>
/// Implementation of IUserManagementService using Firebase Admin SDK
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(ILogger<UserManagementService> logger)
    {
        _logger = logger;
    }

    public async Task SetUserRoleAsync(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be null or empty", nameof(role));
        }

        // Validate role is one of the allowed values
        var allowedRoles = new[] { AuthorizationPolicies.UserRole, AuthorizationPolicies.PremiumUserRole, AuthorizationPolicies.AdminRole };
        if (!allowedRoles.Contains(role))
        {
            throw new ArgumentException($"Invalid role. Allowed values: {string.Join(", ", allowedRoles)}", nameof(role));
        }

        try
        {
            var claims = new Dictionary<string, object>
            {
                { "role", role }
            };

            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(userId, claims);

            _logger.LogInformation("Set role '{Role}' for user {UserId}", role, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting role '{Role}' for user {UserId}", role, userId);
            throw;
        }
    }

    public async Task MakeUserAdminAsync(string userId)
    {
        _logger.LogInformation("Making user {UserId} an admin", userId);
        await SetUserRoleAsync(userId, AuthorizationPolicies.AdminRole);
    }

    public async Task UpgradeToPremiumAsync(string userId)
    {
        _logger.LogInformation("Upgrading user {UserId} to premium", userId);
        await SetUserRoleAsync(userId, AuthorizationPolicies.PremiumUserRole);
    }

    public async Task DowngradeToUserAsync(string userId)
    {
        _logger.LogInformation("Downgrading user {UserId} to regular user", userId);
        await SetUserRoleAsync(userId, AuthorizationPolicies.UserRole);
    }

    public async Task<IDictionary<string, object>> GetUserClaimsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        try
        {
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userId);

            // Convert IReadOnlyDictionary to IDictionary
            if (userRecord.CustomClaims == null)
            {
                return new Dictionary<string, object>();
            }

            return new Dictionary<string, object>(userRecord.CustomClaims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting claims for user {UserId}", userId);
            throw;
        }
    }
}
