namespace QuestionRandomizer.UnitTests.Commands.Questions;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.Questions.UpdateQuestionsBatch;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for UpdateQuestionsBatchCommandHandler
/// </summary>
public class UpdateQuestionsBatchCommandHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateQuestionsBatchCommandHandler _handler;

    public UpdateQuestionsBatchCommandHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new UpdateQuestionsBatchCommandHandler(
            _questionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidBatchCommand_UpdatesMultipleQuestionsSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new UpdateQuestionsBatchCommand
        {
            Questions = new List<UpdateQuestionRequest>
            {
                new()
                {
                    Id = "question-1",
                    QuestionText = "Updated Question 1",
                    Answer = "Updated Answer 1",
                    AnswerPl = "Zaktualizowana Odpowiedź 1",
                    Tags = new List<string> { "updated-tag1" }
                },
                new()
                {
                    Id = "question-2",
                    QuestionText = "Updated Question 2",
                    Answer = "Updated Answer 2",
                    AnswerPl = "Zaktualizowana Odpowiedź 2",
                    Tags = new List<string> { "updated-tag2" }
                },
                new()
                {
                    Id = "question-3",
                    QuestionText = "Updated Question 3",
                    Answer = "Updated Answer 3",
                    AnswerPl = "Zaktualizowana Odpowiedź 3",
                    Tags = new List<string> { "updated-tag3" }
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.UpdateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.UpdateManyAsync(
                It.Is<List<Question>>(q =>
                    q.Count == 3 &&
                    q.All(question => question.UserId == userId) &&
                    q[0].Id == "question-1" &&
                    q[1].Id == "question-2" &&
                    q[2].Id == "question-3"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionsWithCategoryIds_UpdatesQuestionsWithCategories()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";

        var command = new UpdateQuestionsBatchCommand
        {
            Questions = new List<UpdateQuestionRequest>
            {
                new()
                {
                    Id = "question-1",
                    QuestionText = "Question 1",
                    Answer = "Answer 1",
                    AnswerPl = "Odpowiedź 1",
                    CategoryId = categoryId
                },
                new()
                {
                    Id = "question-2",
                    QuestionText = "Question 2",
                    Answer = "Answer 2",
                    AnswerPl = "Odpowiedź 2",
                    CategoryId = categoryId
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.UpdateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.UpdateManyAsync(
                It.Is<List<Question>>(q => q.All(question => question.CategoryId == categoryId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionsWithQualificationIds_UpdatesQuestionsWithQualifications()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-789";

        var command = new UpdateQuestionsBatchCommand
        {
            Questions = new List<UpdateQuestionRequest>
            {
                new()
                {
                    Id = "question-1",
                    QuestionText = "Question 1",
                    Answer = "Answer 1",
                    AnswerPl = "Odpowiedź 1",
                    QualificationId = qualificationId
                },
                new()
                {
                    Id = "question-2",
                    QuestionText = "Question 2",
                    Answer = "Answer 2",
                    AnswerPl = "Odpowiedź 2",
                    QualificationId = qualificationId
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.UpdateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.UpdateManyAsync(
                It.Is<List<Question>>(q => q.All(question => question.QualificationId == qualificationId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyQuestionsList_UpdatesNoQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new UpdateQuestionsBatchCommand
        {
            Questions = new List<UpdateQuestionRequest>()
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.UpdateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.UpdateManyAsync(
                It.Is<List<Question>>(q => q.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionsWithTags_PreservesTagsInUpdatedQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new UpdateQuestionsBatchCommand
        {
            Questions = new List<UpdateQuestionRequest>
            {
                new()
                {
                    Id = "question-1",
                    QuestionText = "Question with tags",
                    Answer = "Answer",
                    AnswerPl = "Odpowiedź",
                    Tags = new List<string> { "tag1", "tag2", "tag3" }
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.UpdateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.UpdateManyAsync(
                It.Is<List<Question>>(q =>
                    q.Count == 1 &&
                    q[0].Tags != null &&
                    q[0].Tags.Count == 3 &&
                    q[0].Tags.Contains("tag1") &&
                    q[0].Tags.Contains("tag2") &&
                    q[0].Tags.Contains("tag3")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidBatchCommand_SetsUpdatedAtTimestamp()
    {
        // Arrange
        var userId = "test-user-123";
        var beforeUpdate = DateTime.UtcNow;

        var command = new UpdateQuestionsBatchCommand
        {
            Questions = new List<UpdateQuestionRequest>
            {
                new()
                {
                    Id = "question-1",
                    QuestionText = "Question 1",
                    Answer = "Answer 1",
                    AnswerPl = "Odpowiedź 1"
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.UpdateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        var afterUpdate = DateTime.UtcNow;

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.UpdateManyAsync(
                It.Is<List<Question>>(q =>
                    q.Count == 1 &&
                    q[0].UpdatedAt >= beforeUpdate &&
                    q[0].UpdatedAt <= afterUpdate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionsWithNullCategoryAndQualification_UpdatesSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new UpdateQuestionsBatchCommand
        {
            Questions = new List<UpdateQuestionRequest>
            {
                new()
                {
                    Id = "question-1",
                    QuestionText = "Question without category and qualification",
                    Answer = "Answer",
                    AnswerPl = "Odpowiedź",
                    CategoryId = null,
                    QualificationId = null
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.UpdateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.UpdateManyAsync(
                It.Is<List<Question>>(q =>
                    q.Count == 1 &&
                    q[0].CategoryId == null &&
                    q[0].QualificationId == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
