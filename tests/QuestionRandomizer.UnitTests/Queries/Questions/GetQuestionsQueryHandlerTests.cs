namespace QuestionRandomizer.UnitTests.Queries.Questions;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Questions.GetQuestions;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetQuestionsQueryHandler
/// </summary>
public class GetQuestionsQueryHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetQuestionsQueryHandler _handler;

    public GetQuestionsQueryHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetQuestionsQueryHandler(
            _questionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_NoCategoryIdFilter_ReturnsAllUserQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQuestionsQuery { CategoryId = null, IsActive = null };

        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Question 1",
                Answer = "Answer 1",
                AnswerPl = "Odpowiedź 1",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Question
            {
                Id = "q2",
                QuestionText = "Question 2",
                Answer = "Answer 2",
                AnswerPl = "Odpowiedź 2",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("q1");
        result[1].Id.Should().Be("q2");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByCategoryIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithCategoryIdFilter_ReturnsFilteredQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";
        var query = new GetQuestionsQuery { CategoryId = categoryId, IsActive = null };

        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Category question 1",
                Answer = "Answer 1",
                AnswerPl = "Odpowiedź 1",
                CategoryId = categoryId,
                CategoryName = "Test Category",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Question
            {
                Id = "q2",
                QuestionText = "Category question 2",
                Answer = "Answer 2",
                AnswerPl = "Odpowiedź 2",
                CategoryId = categoryId,
                CategoryName = "Test Category",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByCategoryIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(q =>
        {
            q.CategoryId.Should().Be(categoryId);
            q.CategoryName.Should().Be("Test Category");
        });

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByCategoryIdAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithIsActiveTrue_ReturnsOnlyActiveQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQuestionsQuery { CategoryId = null, IsActive = true };

        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Active question",
                Answer = "Answer 1",
                AnswerPl = "Odpowiedź 1",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Question
            {
                Id = "q2",
                QuestionText = "Inactive question",
                Answer = "Answer 2",
                AnswerPl = "Odpowiedź 2",
                UserId = userId,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Question
            {
                Id = "q3",
                QuestionText = "Another active question",
                Answer = "Answer 3",
                AnswerPl = "Odpowiedź 3",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(q => q.IsActive.Should().BeTrue());
        result.Should().NotContain(q => q.Id == "q2");
    }

    [Fact]
    public async Task Handle_WithIsActiveFalse_ReturnsOnlyInactiveQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQuestionsQuery { CategoryId = null, IsActive = false };

        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Active question",
                Answer = "Answer 1",
                AnswerPl = "Odpowiedź 1",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Question
            {
                Id = "q2",
                QuestionText = "Inactive question",
                Answer = "Answer 2",
                AnswerPl = "Odpowiedź 2",
                UserId = userId,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("q2");
        result[0].IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithBothFilters_ReturnsCategoryAndActiveFiltered()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";
        var query = new GetQuestionsQuery { CategoryId = categoryId, IsActive = true };

        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Active category question",
                Answer = "Answer 1",
                AnswerPl = "Odpowiedź 1",
                CategoryId = categoryId,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Question
            {
                Id = "q2",
                QuestionText = "Inactive category question",
                Answer = "Answer 2",
                AnswerPl = "Odpowiedź 2",
                CategoryId = categoryId,
                UserId = userId,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByCategoryIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("q1");
        result[0].IsActive.Should().BeTrue();
        result[0].CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task Handle_NoQuestionsFound_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQuestionsQuery { CategoryId = null, IsActive = null };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_QuestionsWithAllFields_MapsAllFieldsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQuestionsQuery { CategoryId = null, IsActive = null };

        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Complete question",
                Answer = "Complete answer",
                AnswerPl = "Pełna odpowiedź",
                CategoryId = "cat-1",
                CategoryName = "Category 1",
                QualificationId = "qual-1",
                QualificationName = "Qualification 1",
                Tags = new List<string> { "tag1", "tag2" },
                UserId = userId,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var dto = result[0];
        dto.Id.Should().Be("q1");
        dto.QuestionText.Should().Be("Complete question");
        dto.Answer.Should().Be("Complete answer");
        dto.AnswerPl.Should().Be("Pełna odpowiedź");
        dto.CategoryId.Should().Be("cat-1");
        dto.CategoryName.Should().Be("Category 1");
        dto.QualificationId.Should().Be("qual-1");
        dto.QualificationName.Should().Be("Qualification 1");
        dto.Tags.Should().BeEquivalentTo(new[] { "tag1", "tag2" });
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        dto.UpdatedAt.Should().Be(new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Handle_EmptyCategoryIdString_TreatsAsNoCategoryFilter()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQuestionsQuery { CategoryId = "", IsActive = null };

        var questions = new List<Question>
        {
            new Question
            {
                Id = "q1",
                QuestionText = "Question 1",
                Answer = "Answer 1",
                AnswerPl = "Odpowiedź 1",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        _questionRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _questionRepositoryMock.Verify(
            x => x.GetByCategoryIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
