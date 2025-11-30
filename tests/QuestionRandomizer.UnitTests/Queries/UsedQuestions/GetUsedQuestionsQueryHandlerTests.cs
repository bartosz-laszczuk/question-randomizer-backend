namespace QuestionRandomizer.UnitTests.Queries.UsedQuestions;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.UsedQuestions.GetUsedQuestions;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetUsedQuestionsQueryHandler
/// </summary>
public class GetUsedQuestionsQueryHandlerTests
{
    private readonly Mock<IUsedQuestionRepository> _usedQuestionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetUsedQuestionsQueryHandler _handler;

    public GetUsedQuestionsQueryHandlerTests()
    {
        _usedQuestionRepositoryMock = new Mock<IUsedQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetUsedQuestionsQueryHandler(
            _usedQuestionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsUsedQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";

        var query = new GetUsedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var usedQuestions = new List<UsedQuestion>
        {
            new()
            {
                Id = "used-1",
                QuestionId = "question-1",
                CategoryId = "category-1",
                CategoryName = "Software Engineering",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "used-2",
                QuestionId = "question-2",
                CategoryId = "category-2",
                CategoryName = "Cloud Computing",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "used-3",
                QuestionId = "question-3",
                CategoryId = "category-3",
                CategoryName = "Database Design",
                CreatedAt = DateTime.UtcNow
            }
        };

        _usedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usedQuestions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Id.Should().Be("used-1");
        result[0].QuestionId.Should().Be("question-1");
        result[0].CategoryName.Should().Be("Software Engineering");
        result[1].CategoryName.Should().Be("Cloud Computing");
        result[2].CategoryName.Should().Be("Database Design");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _usedQuestionRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoUsedQuestions_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";

        var query = new GetUsedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsedQuestion>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsMappedDtos()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var now = DateTime.UtcNow;

        var query = new GetUsedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var usedQuestions = new List<UsedQuestion>
        {
            new()
            {
                Id = "used-999",
                QuestionId = "question-999",
                CategoryId = "category-999",
                CategoryName = "Test Category",
                CreatedAt = now
            }
        };

        _usedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usedQuestions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("used-999");
        result[0].QuestionId.Should().Be("question-999");
        result[0].CategoryId.Should().Be("category-999");
        result[0].CategoryName.Should().Be("Test Category");
        result[0].CreatedAt.Should().Be(now);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "different-random-999";

        var query = new GetUsedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsedQuestion>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _usedQuestionRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidQuery_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var randomizationId = "random-456";

        var query = new GetUsedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsedQuestion>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _usedQuestionRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidQuery_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var cancellationToken = new CancellationToken();

        var query = new GetUsedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, cancellationToken))
            .ReturnsAsync(new List<UsedQuestion>());

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();

        _usedQuestionRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, cancellationToken),
            Times.Once);
    }
}
