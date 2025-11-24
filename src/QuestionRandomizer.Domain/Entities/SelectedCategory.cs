namespace QuestionRandomizer.Domain.Entities;

/// <summary>
/// Represents a category selected for a randomization session
/// </summary>
public class SelectedCategory
{
    /// <summary>
    /// Unique identifier for the selected category entry (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the category document ID
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// Denormalized category name for quick access
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// When this category was added to the session
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
