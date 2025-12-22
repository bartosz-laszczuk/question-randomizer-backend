namespace QuestionRandomizer.Modules.Randomization.Domain.Entities;

/// <summary>
/// Represents a question that has been postponed for later in a randomization session
/// </summary>
public class PostponedQuestion
{
    /// <summary>
    /// Unique identifier for the postponed question entry (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the question document ID
    /// </summary>
    public string QuestionId { get; set; } = string.Empty;

    /// <summary>
    /// When this question was postponed
    /// </summary>
    public DateTime? Timestamp { get; set; }
}
