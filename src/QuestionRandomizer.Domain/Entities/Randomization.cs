namespace QuestionRandomizer.Domain.Entities;

/// <summary>
/// Represents a randomization session for practicing questions
/// </summary>
public class Randomization
{
    /// <summary>
    /// Unique identifier for the randomization session (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who owns this session
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the session is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether to show the answer in the UI
    /// </summary>
    public bool ShowAnswer { get; set; }

    /// <summary>
    /// Status of the session (Ongoing, Completed)
    /// </summary>
    public string Status { get; set; } = "Ongoing";

    /// <summary>
    /// ID of the current question being displayed
    /// </summary>
    public string? CurrentQuestionId { get; set; }

    /// <summary>
    /// List of category IDs selected for this session
    /// </summary>
    public List<string>? SelectedCategoryIds { get; set; }

    /// <summary>
    /// List of question IDs that have been used in this session
    /// </summary>
    public List<string>? UsedQuestionIds { get; set; }

    /// <summary>
    /// List of question IDs that have been postponed
    /// </summary>
    public List<string>? PostponedQuestionIds { get; set; }

    /// <summary>
    /// When the session was created (managed by Firestore)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the session was last updated (managed by Firestore)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
