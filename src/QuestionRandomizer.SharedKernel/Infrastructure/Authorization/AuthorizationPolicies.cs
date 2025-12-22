namespace QuestionRandomizer.SharedKernel.Infrastructure.Authorization;

/// <summary>
/// Defines authorization policies, roles, and permissions for the application
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy names for authorization
    /// </summary>
    public const string UserPolicy = "UserPolicy";
    public const string PremiumUserPolicy = "PremiumUserPolicy";
    public const string AdminPolicy = "AdminPolicy";

    /// <summary>
    /// Role names (must match Firebase custom claims)
    /// </summary>
    public const string UserRole = "User";
    public const string PremiumUserRole = "PremiumUser";
    public const string AdminRole = "Admin";

    /// <summary>
    /// Permission constants for fine-grained authorization
    /// </summary>
    public static class Permissions
    {
        // Question permissions
        public const string QuestionsCreate = "questions:create";
        public const string QuestionsRead = "questions:read";
        public const string QuestionsUpdate = "questions:update";
        public const string QuestionsDelete = "questions:delete";
        public const string QuestionsReadAll = "questions:read:all"; // Admin only

        // Category permissions
        public const string CategoriesManage = "categories:manage";

        // Qualification permissions
        public const string QualificationsManage = "qualifications:manage";

        // AI Agent permissions
        public const string AgentExecuteBasic = "agent:execute:basic";
        public const string AgentExecuteAdvanced = "agent:execute:advanced";
        public const string AgentStreaming = "agent:streaming";

        // Conversation permissions
        public const string ConversationsManage = "conversations:manage";

        // Admin permissions
        public const string UsersManage = "users:manage";
        public const string AnalyticsView = "analytics:view";
        public const string SystemManage = "system:manage";
    }
}
