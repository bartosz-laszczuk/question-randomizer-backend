namespace QuestionRandomizer.UnitTests.Commands.Questions;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestionsBatch;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for CreateQuestionsBatchCommandHandler
/// </summary>
public class CreateQuestionsBatchCommandHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateQuestionsBatchCommandHandler _handler;

    public CreateQuestionsBatchCommandHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateQuestionsBatchCommandHandler(
            _questionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidBatchCommand_CreatesMultipleQuestionsSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new()
                {
                    QuestionText = "What is SOLID?",
                    Answer = "SOLID is a set of principles...",
                    AnswerPl = "SOLID to zestaw zasad...",
                    Tags = new List<string> { "design-patterns" }
                },
                new()
                {
                    QuestionText = "What is DDD?",
                    Answer = "Domain-Driven Design...",
                    AnswerPl = "Projektowanie kierowane domeną...",
                    Tags = new List<string> { "architecture" }
                },
                new()
                {
                    QuestionText = "What is CQRS?",
                    Answer = "Command Query Responsibility Segregation...",
                    AnswerPl = "Segregacja odpowiedzialności poleceń i zapytań...",
                    Tags = new List<string> { "patterns" }
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQuestions = new List<Question>
        {
            new()
            {
                Id = "question-1",
                QuestionText = command.Questions[0].QuestionText,
                Answer = command.Questions[0].Answer,
                AnswerPl = command.Questions[0].AnswerPl,
                UserId = userId,
                IsActive = true,
                Tags = command.Questions[0].Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "question-2",
                QuestionText = command.Questions[1].QuestionText,
                Answer = command.Questions[1].Answer,
                AnswerPl = command.Questions[1].AnswerPl,
                UserId = userId,
                IsActive = true,
                Tags = command.Questions[1].Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "question-3",
                QuestionText = command.Questions[2].QuestionText,
                Answer = command.Questions[2].Answer,
                AnswerPl = command.Questions[2].AnswerPl,
                UserId = userId,
                IsActive = true,
                Tags = command.Questions[2].Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _questionRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestions);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].QuestionText.Should().Be(command.Questions[0].QuestionText);
        result[1].QuestionText.Should().Be(command.Questions[1].QuestionText);
        result[2].QuestionText.Should().Be(command.Questions[2].QuestionText);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Question>>(q => q.Count == 3 && q.All(question => question.UserId == userId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionsWithCategoryIds_CreatesQuestionsWithCategories()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";

        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new()
                {
                    QuestionText = "Question 1",
                    Answer = "Answer 1",
                    AnswerPl = "Odpowiedź 1",
                    CategoryId = categoryId
                },
                new()
                {
                    QuestionText = "Question 2",
                    Answer = "Answer 2",
                    AnswerPl = "Odpowiedź 2",
                    CategoryId = categoryId
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQuestions = new List<Question>
        {
            new()
            {
                Id = "question-1",
                QuestionText = command.Questions[0].QuestionText,
                Answer = command.Questions[0].Answer,
                AnswerPl = command.Questions[0].AnswerPl,
                CategoryId = categoryId,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "question-2",
                QuestionText = command.Questions[1].QuestionText,
                Answer = command.Questions[1].Answer,
                AnswerPl = command.Questions[1].AnswerPl,
                CategoryId = categoryId,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _questionRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestions);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(q => q.CategoryId.Should().Be(categoryId));

        _questionRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Question>>(q => q.All(question => question.CategoryId == categoryId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionsWithQualificationIds_CreatesQuestionsWithQualifications()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-789";

        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new()
                {
                    QuestionText = "Question 1",
                    Answer = "Answer 1",
                    AnswerPl = "Odpowiedź 1",
                    QualificationId = qualificationId
                },
                new()
                {
                    QuestionText = "Question 2",
                    Answer = "Answer 2",
                    AnswerPl = "Odpowiedź 2",
                    QualificationId = qualificationId
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQuestions = new List<Question>
        {
            new()
            {
                Id = "question-1",
                QuestionText = command.Questions[0].QuestionText,
                Answer = command.Questions[0].Answer,
                AnswerPl = command.Questions[0].AnswerPl,
                QualificationId = qualificationId,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "question-2",
                QuestionText = command.Questions[1].QuestionText,
                Answer = command.Questions[1].Answer,
                AnswerPl = command.Questions[1].AnswerPl,
                QualificationId = qualificationId,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _questionRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestions);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(q => q.QualificationId.Should().Be(qualificationId));

        _questionRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Question>>(q => q.All(question => question.QualificationId == qualificationId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyQuestionsList_CreatesNoQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>()
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _questionRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Question>>(q => q.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuestionsWithTags_PreservesTagsInCreatedQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new()
                {
                    QuestionText = "Question with tags",
                    Answer = "Answer",
                    AnswerPl = "Odpowiedź",
                    Tags = new List<string> { "tag1", "tag2", "tag3" }
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQuestions = new List<Question>
        {
            new()
            {
                Id = "question-1",
                QuestionText = command.Questions[0].QuestionText,
                Answer = command.Questions[0].Answer,
                AnswerPl = command.Questions[0].AnswerPl,
                Tags = command.Questions[0].Tags,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _questionRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestions);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Tags.Should().BeEquivalentTo(new List<string> { "tag1", "tag2", "tag3" });
    }

    [Fact]
    public async Task Handle_ValidBatchCommand_SetsIsActiveToTrue()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new()
                {
                    QuestionText = "Question 1",
                    Answer = "Answer 1",
                    AnswerPl = "Odpowiedź 1"
                }
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQuestions = new List<Question>
        {
            new()
            {
                Id = "question-1",
                QuestionText = command.Questions[0].QuestionText,
                Answer = command.Questions[0].Answer,
                AnswerPl = command.Questions[0].AnswerPl,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _questionRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Question>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQuestions);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].IsActive.Should().BeTrue();

        _questionRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Question>>(q => q.All(question => question.IsActive)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
