namespace QuestionRandomizer.Domain.Entities;

/// <summary>
/// Represents a question that has been used (shown) in a randomization session
/// </summary>
public class UsedQuestion
{
    /// <summary>
    /// Unique identifier for the used question entry (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the question document ID
    /// </summary>
    public string QuestionId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the category document ID (nullable)
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Denormalized category name for quick access
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// When this question was used in the session
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
