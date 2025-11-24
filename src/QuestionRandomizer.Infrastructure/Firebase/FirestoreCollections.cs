namespace QuestionRandomizer.Infrastructure.Firebase;

/// <summary>
/// Constants for Firestore collection names
/// </summary>
public static class FirestoreCollections
{
    public const string Users = "users";
    public const string Questions = "questions";
    public const string Categories = "categories";
    public const string Qualifications = "qualifications";
    public const string Conversations = "conversations";
    public const string Messages = "messages";
    public const string Randomizations = "randomizations";

    // Subcollections (nested under Randomizations)
    public const string SelectedCategories = "selectedCategories";
    public const string UsedQuestions = "usedQuestions";
    public const string PostponedQuestions = "postponedQuestions";
}
