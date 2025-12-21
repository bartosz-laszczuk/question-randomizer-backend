namespace QuestionRandomizer.Application.Common;

using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;

/// <summary>
/// Base class for handlers that require authorization logic
/// </summary>
public abstract class AuthorizedHandlerBase
{
    protected readonly ICurrentUserService CurrentUserService;

    protected AuthorizedHandlerBase(ICurrentUserService currentUserService)
    {
        CurrentUserService = currentUserService;
    }

    /// <summary>
    /// Ensures the current user owns the resource
    /// </summary>
    /// <param name="resourceUserId">The userId of the resource owner</param>
    /// <exception cref="UnauthorizedException">Thrown when user does not own the resource</exception>
    protected void EnsureOwnership(string resourceUserId)
    {
        var currentUserId = CurrentUserService.GetUserId();

        // Admin can access any resource
        if (CurrentUserService.IsAdmin())
            return;

        if (resourceUserId != currentUserId)
        {
            throw new UnauthorizedException("You do not have permission to access this resource");
        }
    }

    /// <summary>
    /// Ensures the current user is an admin
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when user is not an admin</exception>
    protected void EnsureAdmin()
    {
        if (!CurrentUserService.IsAdmin())
        {
            throw new UnauthorizedException("This operation requires admin privileges");
        }
    }

    /// <summary>
    /// Ensures the current user is premium or admin
    /// </summary>
    /// <exception cref="UnauthorizedException">Thrown when user is not premium or admin</exception>
    protected void EnsurePremium()
    {
        if (!CurrentUserService.IsPremiumUser() && !CurrentUserService.IsAdmin())
        {
            throw new UnauthorizedException("This feature requires a premium subscription");
        }
    }

    /// <summary>
    /// Gets the current user ID (convenience method)
    /// </summary>
    /// <returns>Current user ID</returns>
    protected string GetCurrentUserId()
    {
        return CurrentUserService.GetUserId();
    }

    /// <summary>
    /// Checks if the current user is an admin (convenience method)
    /// </summary>
    /// <returns>True if admin, false otherwise</returns>
    protected bool IsAdmin()
    {
        return CurrentUserService.IsAdmin();
    }

    /// <summary>
    /// Checks if the current user is premium (convenience method)
    /// </summary>
    /// <returns>True if premium or admin, false otherwise</returns>
    protected bool IsPremium()
    {
        return CurrentUserService.IsPremiumUser();
    }
}
