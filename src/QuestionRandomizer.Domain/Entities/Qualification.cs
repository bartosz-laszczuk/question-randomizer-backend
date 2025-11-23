namespace QuestionRandomizer.Domain.Entities;

/// <summary>
/// Represents a job qualification that questions can be associated with
/// </summary>
public class Qualification
{
    /// <summary>
    /// Unique identifier for the qualification (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The qualification name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the qualification
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the qualification is active (not deleted)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ID of the user who owns this qualification
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// When the qualification was created (managed by Firestore)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the qualification was last updated (managed by Firestore)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
