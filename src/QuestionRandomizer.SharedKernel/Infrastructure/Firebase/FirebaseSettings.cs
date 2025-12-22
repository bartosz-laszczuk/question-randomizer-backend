namespace QuestionRandomizer.SharedKernel.Infrastructure.Firebase;

/// <summary>
/// Configuration settings for Firebase
/// </summary>
public class FirebaseSettings
{
    /// <summary>
    /// Firebase project ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Path to Firebase Admin SDK credentials JSON file
    /// </summary>
    public string CredentialsPath { get; set; } = string.Empty;
}
