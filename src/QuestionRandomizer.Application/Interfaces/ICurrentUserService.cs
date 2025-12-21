namespace QuestionRandomizer.Application.Interfaces;

/// <summary>
/// Service for accessing the current authenticated user and their authorization context
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the ID of the currently authenticated user
    /// </summary>
    /// <returns>User ID from authentication context</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when no user is authenticated</exception>
    string GetUserId();

    /// <summary>
    /// Gets the email of the currently authenticated user
    /// </summary>
    /// <returns>User email from authentication context, or null if not available</returns>
    string? GetUserEmail();

    /// <summary>
    /// Gets the role of the currently authenticated user
    /// </summary>
    /// <returns>User role from authentication claims (defaults to "User" if no role claim exists)</returns>
    string GetUserRole();

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    /// <param name="role">Role name to check</param>
    /// <returns>True if user has the specified role, false otherwise</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Checks if the current user is an admin
    /// </summary>
    /// <returns>True if user has Admin role, false otherwise</returns>
    bool IsAdmin();

    /// <summary>
    /// Checks if the current user is a premium user or admin
    /// </summary>
    /// <returns>True if user has PremiumUser or Admin role, false otherwise</returns>
    bool IsPremiumUser();
}
