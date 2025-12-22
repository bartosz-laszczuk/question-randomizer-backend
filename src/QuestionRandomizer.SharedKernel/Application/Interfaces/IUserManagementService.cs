namespace QuestionRandomizer.SharedKernel.Application.Interfaces;

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
