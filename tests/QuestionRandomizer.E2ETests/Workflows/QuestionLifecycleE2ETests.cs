namespace QuestionRandomizer.E2ETests.Workflows;

using System.Net;
using Bogus;
using FluentAssertions;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.E2ETests.Infrastructure;

/// <summary>
/// End-to-End tests for the complete Question lifecycle workflow
/// Tests the entire journey: Create → Read → Update → Delete
/// </summary>
public class QuestionLifecycleE2ETests : E2ETestBase
{
    private readonly Faker _faker;

    public QuestionLifecycleE2ETests(E2ETestWebApplicationFactory factory) : base(factory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task CompleteQuestionLifecycle_ShouldSucceed()
    {
        // Arrange - Create supporting entities first
        var categoryName = CreateTestString("E2E_Category");
        var qualificationName = CreateTestString("E2E_Qualification");

        // Step 1: Create a category
        var createCategoryRequest = new { Name = categoryName };
        var category = await PostAsync<object, CategoryDto>("/api/categories", createCategoryRequest);

        category.Should().NotBeNull();
        category!.Id.Should().NotBeEmpty();
        category.Name.Should().Be(categoryName);
        category.IsActive.Should().BeTrue();

        // Step 2: Create a qualification
        var createQualificationRequest = new { Name = qualificationName };
        var qualification = await PostAsync<object, QualificationDto>("/api/qualifications", createQualificationRequest);

        qualification.Should().NotBeNull();
        qualification!.Id.Should().NotBeEmpty();
        qualification.Name.Should().Be(qualificationName);
        qualification.IsActive.Should().BeTrue();

        // Step 3: Create a question with category and qualification
        var questionText = CreateTestString("What is");
        var answer = "Test Answer";
        var answerPl = "Odpowiedź Testowa";
        var tags = new List<string> { "test", "e2e" };

        var createQuestionRequest = new
        {
            QuestionText = questionText,
            Answer = answer,
            AnswerPl = answerPl,
            CategoryId = category.Id,
            QualificationId = qualification.Id,
            Tags = tags
        };

        var createdQuestion = await PostAsync<object, QuestionDto>("/api/questions", createQuestionRequest);

        createdQuestion.Should().NotBeNull();
        createdQuestion!.Id.Should().NotBeEmpty();
        createdQuestion.QuestionText.Should().Be(questionText);
        createdQuestion.Answer.Should().Be(answer);
        createdQuestion.AnswerPl.Should().Be(answerPl);
        createdQuestion.CategoryId.Should().Be(category.Id);
        createdQuestion.CategoryName.Should().Be(categoryName);
        createdQuestion.QualificationId.Should().Be(qualification.Id);
        createdQuestion.QualificationName.Should().Be(qualificationName);
        createdQuestion.IsActive.Should().BeTrue();
        createdQuestion.Tags.Should().BeEquivalentTo(tags);
        createdQuestion.CreatedAt.Should().NotBeNull();

        // Step 4: Get the question by ID
        var retrievedQuestion = await GetAsync<QuestionDto>($"/api/questions/{createdQuestion.Id}");

        retrievedQuestion.Should().NotBeNull();
        retrievedQuestion.Should().BeEquivalentTo(createdQuestion);

        // Step 5: Update the question
        var updatedQuestionText = CreateTestString("Updated: What is");
        var updatedAnswer = "Updated Answer";
        var updatedTags = new List<string> { "updated", "e2e", "test" };

        var updateQuestionRequest = new
        {
            Id = createdQuestion.Id,
            QuestionText = updatedQuestionText,
            Answer = updatedAnswer,
            AnswerPl = answerPl,
            CategoryId = category.Id,
            QualificationId = qualification.Id,
            Tags = updatedTags
        };

        var updatedQuestion = await PutAsync<object, QuestionDto>(
            $"/api/questions/{createdQuestion.Id}",
            updateQuestionRequest);

        updatedQuestion.Should().NotBeNull();
        updatedQuestion!.Id.Should().Be(createdQuestion.Id);
        updatedQuestion.QuestionText.Should().Be(updatedQuestionText);
        updatedQuestion.Answer.Should().Be(updatedAnswer);
        updatedQuestion.Tags.Should().BeEquivalentTo(updatedTags);
        updatedQuestion.UpdatedAt.Should().NotBeNull();
        updatedQuestion.UpdatedAt.Should().BeAfter(updatedQuestion.CreatedAt!.Value);

        // Step 6: Get all questions - verify the updated one is there
        var allQuestions = await GetAsync<List<QuestionDto>>("/api/questions");

        allQuestions.Should().NotBeNull();
        allQuestions.Should().Contain(q => q.Id == updatedQuestion.Id);

        var questionInList = allQuestions!.First(q => q.Id == updatedQuestion.Id);
        questionInList.QuestionText.Should().Be(updatedQuestionText);
        questionInList.Answer.Should().Be(updatedAnswer);

        // Step 7: Delete the question (soft delete)
        var deleteResponse = await DeleteAsync($"/api/questions/{createdQuestion.Id}");

        AssertSuccess(deleteResponse);

        // Step 8: Verify 404 when getting deleted question
        var getDeletedResponse = await GetAsync($"/api/questions/{createdQuestion.Id}");

        AssertNotFound(getDeletedResponse);

        // Step 9: Verify deleted question is not in the list
        var questionsAfterDelete = await GetAsync<List<QuestionDto>>("/api/questions");

        questionsAfterDelete.Should().NotBeNull();
        questionsAfterDelete.Should().NotContain(q => q.Id == createdQuestion.Id);
    }

    [Fact]
    public async Task CreateQuestion_WithoutCategoryAndQualification_ShouldSucceed()
    {
        // Arrange
        var questionText = CreateTestString("Simple question");
        var answer = "Simple answer";
        var answerPl = "Prosta odpowiedź";

        var createRequest = new
        {
            QuestionText = questionText,
            Answer = answer,
            AnswerPl = answerPl
        };

        // Act
        var createdQuestion = await PostAsync<object, QuestionDto>("/api/questions", createRequest);

        // Assert
        createdQuestion.Should().NotBeNull();
        createdQuestion!.QuestionText.Should().Be(questionText);
        createdQuestion.Answer.Should().Be(answer);
        createdQuestion.AnswerPl.Should().Be(answerPl);
        createdQuestion.CategoryId.Should().BeNull();
        createdQuestion.QualificationId.Should().BeNull();
        createdQuestion.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateQuestion_ChangingCategoryAndQualification_ShouldSucceed()
    {
        // Arrange - Create two categories and qualifications
        var category1 = await PostAsync<object, CategoryDto>(
            "/api/categories",
            new { Name = CreateTestString("Category1") });
        var category2 = await PostAsync<object, CategoryDto>(
            "/api/categories",
            new { Name = CreateTestString("Category2") });

        var qualification1 = await PostAsync<object, QualificationDto>(
            "/api/qualifications",
            new { Name = CreateTestString("Qualification1") });
        var qualification2 = await PostAsync<object, QualificationDto>(
            "/api/qualifications",
            new { Name = CreateTestString("Qualification2") });

        // Create question with first category and qualification
        var createRequest = new
        {
            QuestionText = CreateTestString("Question"),
            Answer = "Answer",
            AnswerPl = "Odpowiedź",
            CategoryId = category1!.Id,
            QualificationId = qualification1!.Id
        };

        var question = await PostAsync<object, QuestionDto>("/api/questions", createRequest);

        // Act - Update to second category and qualification
        var updateRequest = new
        {
            Id = question!.Id,
            QuestionText = question.QuestionText,
            Answer = question.Answer,
            AnswerPl = question.AnswerPl,
            CategoryId = category2!.Id,
            QualificationId = qualification2!.Id
        };

        var updatedQuestion = await PutAsync<object, QuestionDto>(
            $"/api/questions/{question.Id}",
            updateRequest);

        // Assert
        updatedQuestion.Should().NotBeNull();
        updatedQuestion!.CategoryId.Should().Be(category2.Id);
        updatedQuestion.CategoryName.Should().Be(category2.Name);
        updatedQuestion.QualificationId.Should().Be(qualification2.Id);
        updatedQuestion.QualificationName.Should().Be(qualification2.Name);
    }

    [Fact]
    public async Task GetQuestion_WithInvalidId_ShouldReturn404()
    {
        // Act
        var response = await GetAsync("/api/questions/non-existent-id");

        // Assert
        AssertNotFound(response);
    }

    [Fact]
    public async Task DeleteQuestion_WithInvalidId_ShouldReturn404()
    {
        // Act
        var response = await DeleteAsync("/api/questions/non-existent-id");

        // Assert
        AssertNotFound(response);
    }

    [Fact]
    public async Task CreateQuestion_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - Empty question text (violates validation)
        var createRequest = new
        {
            QuestionText = "",
            Answer = "Answer",
            AnswerPl = "Odpowiedź"
        };

        // Act
        var response = await PostAsync("/api/questions", createRequest);

        // Assert
        AssertBadRequest(response);
    }
}
