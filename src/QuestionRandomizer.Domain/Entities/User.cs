namespace QuestionRandomizer.Domain.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user (Firebase Auth UID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the user account was created (managed by Firestore)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the user account was last updated (managed by Firestore)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
