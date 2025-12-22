namespace QuestionRandomizer.E2ETests.Workflows;

using System.Net;
using Bogus;
using FluentAssertions;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.E2ETests.Infrastructure;

/// <summary>
/// End-to-End tests for the complete Randomization workflow
/// Tests randomization creation, question tracking (used/postponed), and session management
/// </summary>
public class RandomizationWorkflowE2ETests : E2ETestBase
{
    private readonly Faker _faker;

    public RandomizationWorkflowE2ETests(E2ETestWebApplicationFactory factory) : base(factory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task CompleteRandomizationWorkflow_ShouldSucceed()
    {
        // Arrange - Create a category and 10 questions
        var categoryName = CreateTestString("E2E_RandomCategory");
        var category = await PostAsync<object, CategoryDto>(
            "/api/categories",
            new { Name = categoryName });

        category.Should().NotBeNull();

        // Create 10 questions in the category
        var questionIds = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var createQuestionRequest = new
            {
                QuestionText = CreateTestString($"Question {i + 1}"),
                Answer = $"Answer {i + 1}",
                AnswerPl = $"Odpowiedź {i + 1}",
                CategoryId = category!.Id
            };

            var question = await PostAsync<object, QuestionDto>("/api/questions", createQuestionRequest);
            question.Should().NotBeNull();
            questionIds.Add(question!.Id);
        }

        questionIds.Should().HaveCount(10);

        // Step 1: Create a randomization session
        var createRandomizationRequest = new { };
        var randomization = await PostAsync<object, RandomizationDto>(
            "/api/randomizations",
            createRandomizationRequest);

        randomization.Should().NotBeNull();
        randomization!.Id.Should().NotBeEmpty();
        randomization.Status.Should().Be("Ongoing");
        randomization.ShowAnswer.Should().BeFalse();
        randomization.CreatedAt.Should().NotBeNull();

        // Step 2: Get the randomization (verify it's active)
        var activeRandomization = await GetAsync<RandomizationDto>("/api/randomizations");
        activeRandomization.Should().NotBeNull();
        activeRandomization!.Id.Should().Be(randomization.Id);

        // Step 3: Mark first question as used
        var addUsedQuestionRequest = new
        {
            RandomizationId = randomization.Id,
            QuestionId = questionIds[0],
            CategoryId = category!.Id,
            CategoryName = category.Name
        };

        var usedQuestion = await PostAsync<object, UsedQuestionDto>(
            $"/api/randomizations/{randomization.Id}/used-questions",
            addUsedQuestionRequest);

        usedQuestion.Should().NotBeNull();
        usedQuestion!.QuestionId.Should().Be(questionIds[0]);
        usedQuestion.CategoryId.Should().Be(category.Id);
        usedQuestion.CategoryName.Should().Be(category.Name);

        // Step 4: Mark second question as postponed
        var addPostponedQuestionRequest = new
        {
            RandomizationId = randomization.Id,
            QuestionId = questionIds[1]
        };

        var postponedQuestion = await PostAsync<object, PostponedQuestionDto>(
            $"/api/randomizations/{randomization.Id}/postponed-questions",
            addPostponedQuestionRequest);

        postponedQuestion.Should().NotBeNull();
        postponedQuestion!.QuestionId.Should().Be(questionIds[1]);
        postponedQuestion.Timestamp.Should().NotBeNull();

        // Step 5: Get used questions - verify the first question is there
        var usedQuestions = await GetAsync<List<UsedQuestionDto>>(
            $"/api/randomizations/{randomization.Id}/used-questions");

        usedQuestions.Should().NotBeNull();
        usedQuestions.Should().HaveCount(1);
        usedQuestions![0].QuestionId.Should().Be(questionIds[0]);

        // Step 6: Get postponed questions - verify the second question is there
        var postponedQuestions = await GetAsync<List<PostponedQuestionDto>>(
            $"/api/randomizations/{randomization.Id}/postponed-questions");

        postponedQuestions.Should().NotBeNull();
        postponedQuestions.Should().HaveCount(1);
        postponedQuestions![0].QuestionId.Should().Be(questionIds[1]);

        // Step 7: Update randomization status to show answer
        var updateRandomizationRequest = new
        {
            Id = randomization.Id,
            ShowAnswer = true,
            Status = "Ongoing",
            CurrentQuestionId = questionIds[2]
        };

        var updatedRandomization = await PutAsync<object, RandomizationDto>(
            $"/api/randomizations/{randomization.Id}",
            updateRandomizationRequest);

        updatedRandomization.Should().NotBeNull();
        updatedRandomization!.ShowAnswer.Should().BeTrue();
        updatedRandomization.CurrentQuestionId.Should().Be(questionIds[2]);

        // Step 8: Complete the randomization
        var completeRandomizationRequest = new
        {
            Id = randomization.Id,
            ShowAnswer = false,
            Status = "Completed",
            CurrentQuestionId = (string?)null
        };

        var completedRandomization = await PutAsync<object, RandomizationDto>(
            $"/api/randomizations/{randomization.Id}",
            completeRandomizationRequest);

        completedRandomization.Should().NotBeNull();
        completedRandomization!.Status.Should().Be("Completed");
        completedRandomization.ShowAnswer.Should().BeFalse();
        completedRandomization.CurrentQuestionId.Should().BeNull();

        // Step 9: Verify we can still retrieve the completed randomization
        var finalRandomization = await GetAsync<RandomizationDto>("/api/randomizations");
        finalRandomization.Should().NotBeNull();
        finalRandomization!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task RandomizationWorkflow_WithMultipleUsedQuestions_ShouldTrackAll()
    {
        // Arrange - Create questions
        var question1 = await CreateTestQuestion("Q1");
        var question2 = await CreateTestQuestion("Q2");
        var question3 = await CreateTestQuestion("Q3");

        // Create randomization
        var randomization = await PostAsync<object, RandomizationDto>(
            "/api/randomizations",
            new { });

        // Act - Mark all questions as used
        await PostAsync<object, UsedQuestionDto>(
            $"/api/randomizations/{randomization!.Id}/used-questions",
            new { RandomizationId = randomization.Id, QuestionId = question1 });

        await PostAsync<object, UsedQuestionDto>(
            $"/api/randomizations/{randomization.Id}/used-questions",
            new { RandomizationId = randomization.Id, QuestionId = question2 });

        await PostAsync<object, UsedQuestionDto>(
            $"/api/randomizations/{randomization.Id}/used-questions",
            new { RandomizationId = randomization.Id, QuestionId = question3 });

        // Assert - Verify all are tracked
        var usedQuestions = await GetAsync<List<UsedQuestionDto>>(
            $"/api/randomizations/{randomization.Id}/used-questions");

        usedQuestions.Should().HaveCount(3);
        usedQuestions.Should().Contain(q => q.QuestionId == question1);
        usedQuestions.Should().Contain(q => q.QuestionId == question2);
        usedQuestions.Should().Contain(q => q.QuestionId == question3);
    }

    [Fact]
    public async Task RandomizationWorkflow_RemoveUsedQuestion_ShouldSucceed()
    {
        // Arrange
        var question = await CreateTestQuestion("TestQuestion");
        var randomization = await PostAsync<object, RandomizationDto>(
            "/api/randomizations",
            new { });

        // Add used question
        await PostAsync<object, UsedQuestionDto>(
            $"/api/randomizations/{randomization!.Id}/used-questions",
            new { RandomizationId = randomization.Id, QuestionId = question });

        // Act - Remove the used question
        var deleteResponse = await DeleteAsync(
            $"/api/randomizations/{randomization.Id}/used-questions/{question}");

        // Assert
        AssertSuccess(deleteResponse);

        var usedQuestions = await GetAsync<List<UsedQuestionDto>>(
            $"/api/randomizations/{randomization.Id}/used-questions");

        usedQuestions.Should().BeEmpty();
    }

    [Fact]
    public async Task RandomizationWorkflow_RemovePostponedQuestion_ShouldSucceed()
    {
        // Arrange
        var question = await CreateTestQuestion("TestQuestion");
        var randomization = await PostAsync<object, RandomizationDto>(
            "/api/randomizations",
            new { });

        // Add postponed question
        await PostAsync<object, PostponedQuestionDto>(
            $"/api/randomizations/{randomization!.Id}/postponed-questions",
            new { RandomizationId = randomization.Id, QuestionId = question });

        // Act - Remove the postponed question
        var deleteResponse = await DeleteAsync(
            $"/api/randomizations/{randomization.Id}/postponed-questions/{question}");

        // Assert
        AssertSuccess(deleteResponse);

        var postponedQuestions = await GetAsync<List<PostponedQuestionDto>>(
            $"/api/randomizations/{randomization.Id}/postponed-questions");

        postponedQuestions.Should().BeEmpty();
    }

    [Fact]
    public async Task RandomizationWorkflow_ClearCurrentQuestion_ShouldSucceed()
    {
        // Arrange
        var question = await CreateTestQuestion("CurrentQuestion");
        var randomization = await PostAsync<object, RandomizationDto>(
            "/api/randomizations",
            new { });

        // Set current question
        await PutAsync<object, RandomizationDto>(
            $"/api/randomizations/{randomization!.Id}",
            new
            {
                Id = randomization.Id,
                ShowAnswer = false,
                Status = "Ongoing",
                CurrentQuestionId = question
            });

        // Act - Clear current question
        var clearResponse = await Client.PostAsync(
            $"/api/randomizations/{randomization.Id}/clear-current-question",
            null);

        // Assert
        AssertSuccess(clearResponse);
        clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify current question is cleared
        var updatedRandomization = await GetAsync<RandomizationDto>("/api/randomizations");
        updatedRandomization!.CurrentQuestionId.Should().BeNull();
    }

    [Fact]
    public async Task GetRandomization_WhenNoActiveSession_ShouldReturn404()
    {
        // Act
        var response = await GetAsync("/api/randomizations");

        // Assert
        AssertNotFound(response);
    }

    [Fact]
    public async Task UpdateRandomization_WithInvalidId_ShouldReturn404()
    {
        // Act
        var response = await PutAsync(
            "/api/randomizations/non-existent-id",
            new
            {
                Id = "non-existent-id",
                ShowAnswer = true,
                Status = "Completed"
            });

        // Assert
        AssertNotFound(response);
    }

    [Fact]
    public async Task AddUsedQuestion_WithInvalidRandomizationId_ShouldReturn404()
    {
        // Arrange
        var question = await CreateTestQuestion("TestQuestion");

        // Act
        var response = await PostAsync(
            "/api/randomizations/non-existent-id/used-questions",
            new
            {
                RandomizationId = "non-existent-id",
                QuestionId = question
            });

        // Assert
        AssertNotFound(response);
    }

    /// <summary>
    /// Helper method to create a test question
    /// </summary>
    private async Task<string> CreateTestQuestion(string prefix)
    {
        var createRequest = new
        {
            QuestionText = CreateTestString(prefix),
            Answer = "Test Answer",
            AnswerPl = "Test Odpowiedź"
        };

        var question = await PostAsync<object, QuestionDto>("/api/questions", createRequest);
        return question!.Id;
    }
}
