namespace QuestionRandomizer.Domain.Entities;

/// <summary>
/// Represents an interview question created by a user
/// </summary>
public class Question
{
    /// <summary>
    /// Unique identifier for the question (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The question text in English
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// The answer text in English
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// The answer text in Polish
    /// </summary>
    public string AnswerPl { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the category document ID (nullable)
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Denormalized category name for quick access
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Reference to the qualification document ID (nullable)
    /// </summary>
    public string? QualificationId { get; set; }

    /// <summary>
    /// Denormalized qualification name for quick access
    /// </summary>
    public string? QualificationName { get; set; }

    /// <summary>
    /// Whether the question is active (not deleted)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ID of the user who owns this question
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Optional tags for categorization
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// When the question was created (managed by Firestore)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the question was last updated (managed by Firestore)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
