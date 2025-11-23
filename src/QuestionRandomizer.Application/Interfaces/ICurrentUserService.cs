namespace QuestionRandomizer.Application.Interfaces;

/// <summary>
/// Service for accessing the current authenticated user
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
}
