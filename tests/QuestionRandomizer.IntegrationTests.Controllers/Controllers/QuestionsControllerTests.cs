namespace QuestionRandomizer.IntegrationTests.Controllers.Controllers;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.Modules.Questions.Domain.Entities;
using QuestionRandomizer.IntegrationTests.Controllers.Infrastructure;

/// <summary>
/// Integration tests for QuestionsController
/// </summary>
public class QuestionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public QuestionsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.ResetMocks();
    }

    [Fact]
    public async Task GetQuestions_ReturnsOkWithQuestions()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Test Question 1",
                Answer = "Test Answer 1",
                AnswerPl = "Testowa Odpowiedź 1",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Question
            {
                Id = "q2",
                QuestionText = "Test Question 2",
                Answer = "Test Answer 2",
                AnswerPl = "Testowa Odpowiedź 2",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _factory.QuestionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var response = await _client.GetAsync("/api/questions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<QuestionDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Id.Should().Be("q1");
        result[1].Id.Should().Be("q2");
    }

    [Fact]
    public async Task GetQuestionById_ExistingQuestion_ReturnsOk()
    {
        // Arrange
        var questionId = "test-question-id";
        var question = new Question
        {
            Id = questionId,
            QuestionText = "Test Question",
            Answer = "Test Answer",
            AnswerPl = "Testowa Odpowiedź",
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.QuestionRepositoryMock
            .Setup(x => x.GetByIdAsync(
                questionId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);

        // Act
        var response = await _client.GetAsync($"/api/questions/{questionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<QuestionDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(questionId);
        result.QuestionText.Should().Be("Test Question");
    }

    [Fact]
    public async Task GetQuestionById_NonExistentQuestion_ReturnsNotFound()
    {
        // Arrange
        var questionId = "non-existent-id";

        _factory.QuestionRepositoryMock
            .Setup(x => x.GetByIdAsync(
                questionId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        // Act
        var response = await _client.GetAsync($"/api/questions/{questionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateQuestion_ValidCommand_ReturnsCreated()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "New Question",
            Answer = "New Answer",
            AnswerPl = "Nowa Odpowiedź"
        };

        var createdQuestion = new Question
        {
            Id = "new-question-id",
            QuestionText = command.QuestionText,
            Answer = command.Answer,
            AnswerPl = command.AnswerPl,
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.QuestionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestion);

        // Act
        var response = await _client.PostAsJsonAsync("/api/questions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<QuestionDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-question-id");
        result.QuestionText.Should().Be(command.QuestionText);
        result.Answer.Should().Be(command.Answer);
        result.AnswerPl.Should().Be(command.AnswerPl);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/Questions/{result.Id}");
    }

    [Fact]
    public async Task CreateQuestion_InvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "", // Invalid - empty
            Answer = "Answer",
            AnswerPl = "Odpowiedź"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/questions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateQuestion_ValidCommand_ReturnsOk()
    {
        // Arrange
        var questionId = "question-to-update";
        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Old Question",
            Answer = "Old Answer",
            AnswerPl = "Stara Odpowiedź",
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Updated Question",
            Answer = "Updated Answer",
            AnswerPl = "Zaktualizowana Odpowiedź",
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = existingQuestion.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.QuestionRepositoryMock
            .Setup(x => x.GetByIdAsync(
                questionId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);

        _factory.QuestionRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new
        {
            Id = questionId,
            QuestionText = "Updated Question",
            Answer = "Updated Answer",
            AnswerPl = "Zaktualizowana Odpowiedź"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/questions/{questionId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<QuestionDto>();
        result.Should().NotBeNull();
        result!.QuestionText.Should().Be("Updated Question");
    }

    [Fact]
    public async Task UpdateQuestion_NonExistentQuestion_ReturnsNotFound()
    {
        // Arrange
        var questionId = "non-existent-id";

        _factory.QuestionRepositoryMock
            .Setup(x => x.GetByIdAsync(
                questionId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        var command = new
        {
            Id = questionId,
            QuestionText = "Updated Question",
            Answer = "Updated Answer",
            AnswerPl = "Zaktualizowana Odpowiedź"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/questions/{questionId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQuestion_ExistingQuestion_ReturnsNoContent()
    {
        // Arrange
        var questionId = "question-to-delete";
        var existingQuestion = new Question
        {
            Id = questionId,
            QuestionText = "Question to delete",
            Answer = "Answer",
            AnswerPl = "Odpowiedź",
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.QuestionRepositoryMock
            .Setup(x => x.GetByIdAsync(
                questionId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuestion);

        _factory.QuestionRepositoryMock
            .Setup(x => x.DeleteAsync(
                questionId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync($"/api/questions/{questionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _factory.QuestionRepositoryMock.Verify(
            x => x.DeleteAsync(
                questionId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteQuestion_NonExistentQuestion_ReturnsNotFound()
    {
        // Arrange
        var questionId = "non-existent-id";

        _factory.QuestionRepositoryMock
            .Setup(x => x.DeleteAsync(
                questionId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _client.DeleteAsync($"/api/questions/{questionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetQuestions_WithCategoryFilter_ReturnsFilteredQuestions()
    {
        // Arrange
        var categoryId = "category-123";
        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Category Question",
                Answer = "Answer",
                AnswerPl = "Odpowiedź",
                CategoryId = categoryId,
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _factory.QuestionRepositoryMock
            .Setup(x => x.GetByCategoryIdAsync(
                categoryId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var response = await _client.GetAsync($"/api/questions?categoryId={categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<QuestionDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetQuestions_WithIsActiveFilter_ReturnsFilteredQuestions()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Active Question",
                Answer = "Answer",
                AnswerPl = "Odpowiedź",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Question
            {
                Id = "q2",
                QuestionText = "Inactive Question",
                Answer = "Answer 2",
                AnswerPl = "Odpowiedź 2",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _factory.QuestionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var response = await _client.GetAsync("/api/questions?isActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<QuestionDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].IsActive.Should().BeTrue();
    }
}
