namespace QuestionRandomizer.Modules.Questions.Domain.Entities;

/// <summary>
/// Represents a category for organizing questions
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the category (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the category
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the category is active (not deleted)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ID of the user who owns this category
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// When the category was created (managed by Firestore)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the category was last updated (managed by Firestore)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
