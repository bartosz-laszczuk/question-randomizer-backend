namespace QuestionRandomizer.E2ETests.Infrastructure;

using QuestionRandomizer.Application.Interfaces;

/// <summary>
/// Test implementation of ICurrentUserService that returns predefined test user information
/// </summary>
public class TestCurrentUserService : ICurrentUserService
{
    public string GetUserId()
    {
        return E2ETestWebApplicationFactory.TestUserId;
    }

    public string GetUserEmail()
    {
        return E2ETestWebApplicationFactory.TestUserEmail;
    }
}
