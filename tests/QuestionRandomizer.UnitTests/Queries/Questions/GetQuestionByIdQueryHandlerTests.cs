namespace QuestionRandomizer.UnitTests.Queries.Questions;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Questions.GetQuestionById;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetQuestionByIdQueryHandler
/// </summary>
public class GetQuestionByIdQueryHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetQuestionByIdQueryHandler _handler;

    public GetQuestionByIdQueryHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetQuestionByIdQueryHandler(
            _questionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingQuestion_ReturnsQuestionDto()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-456";
        var query = new GetQuestionByIdQuery { Id = questionId };

        var question = new Question
        {
            Id = questionId,
            QuestionText = "What is CQRS?",
            Answer = "CQRS stands for Command Query Responsibility Segregation...",
            AnswerPl = "CQRS to akronim od Command Query Responsibility Segregation...",
            CategoryId = "category-789",
            CategoryName = "Architecture",
            QualificationId = "qual-123",
            QualificationName = "Software Architect",
            IsActive = true,
            UserId = userId,
            Tags = new List<string> { "architecture", "patterns" },
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(questionId);
        result.QuestionText.Should().Be("What is CQRS?");
        result.Answer.Should().Be("CQRS stands for Command Query Responsibility Segregation...");
        result.AnswerPl.Should().Be("CQRS to akronim od Command Query Responsibility Segregation...");
        result.CategoryId.Should().Be("category-789");
        result.CategoryName.Should().Be("Architecture");
        result.QualificationId.Should().Be("qual-123");
        result.QualificationName.Should().Be("Software Architect");
        result.IsActive.Should().BeTrue();
        result.Tags.Should().BeEquivalentTo(new[] { "architecture", "patterns" });
        result.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        result.UpdatedAt.Should().Be(new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc));

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "non-existent-question";
        var query = new GetQuestionByIdQuery { Id = questionId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Question with ID '{questionId}' was not found");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionWithoutOptionalFields_ReturnsQuestionDto()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-minimal";
        var query = new GetQuestionByIdQuery { Id = questionId };

        var question = new Question
        {
            Id = questionId,
            QuestionText = "Simple question",
            Answer = "Simple answer",
            AnswerPl = "Prosta odpowiedź",
            CategoryId = null,
            CategoryName = null,
            QualificationId = null,
            QualificationName = null,
            IsActive = true,
            UserId = userId,
            Tags = null,
            CreatedAt = null,
            UpdatedAt = null
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(questionId);
        result.CategoryId.Should().BeNull();
        result.CategoryName.Should().BeNull();
        result.QualificationId.Should().BeNull();
        result.QualificationName.Should().BeNull();
        result.Tags.Should().BeNull();
        result.CreatedAt.Should().BeNull();
        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InactiveQuestion_ReturnsInactiveQuestionDto()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-inactive";
        var query = new GetQuestionByIdQuery { Id = questionId };

        var question = new Question
        {
            Id = questionId,
            QuestionText = "Inactive question",
            Answer = "Answer to inactive question",
            AnswerPl = "Odpowiedź na nieaktywne pytanie",
            IsActive = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DifferentUser_UsesCurrentUserService()
    {
        // Arrange
        var userId1 = "user-1";
        var userId2 = "user-2";
        var questionId = "question-789";
        var query = new GetQuestionByIdQuery { Id = questionId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId1);
        _questionRepositoryMock
            .Setup(x => x.GetByIdAsync(questionId, userId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByIdAsync(questionId, userId1, It.IsAny<CancellationToken>()),
            Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByIdAsync(questionId, userId2, It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
