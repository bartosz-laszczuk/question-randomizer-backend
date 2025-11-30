namespace QuestionRandomizer.UnitTests.Queries.PostponedQuestions;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.PostponedQuestions.GetPostponedQuestions;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetPostponedQuestionsQueryHandler
/// </summary>
public class GetPostponedQuestionsQueryHandlerTests
{
    private readonly Mock<IPostponedQuestionRepository> _postponedQuestionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetPostponedQuestionsQueryHandler _handler;

    public GetPostponedQuestionsQueryHandlerTests()
    {
        _postponedQuestionRepositoryMock = new Mock<IPostponedQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetPostponedQuestionsQueryHandler(
            _postponedQuestionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsPostponedQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";

        var query = new GetPostponedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var postponedQuestions = new List<PostponedQuestion>
        {
            new()
            {
                Id = "postponed-1",
                QuestionId = "question-1",
                Timestamp = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = "postponed-2",
                QuestionId = "question-2",
                Timestamp = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = "postponed-3",
                QuestionId = "question-3",
                Timestamp = DateTime.UtcNow.AddHours(-3)
            }
        };

        _postponedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(postponedQuestions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Id.Should().Be("postponed-1");
        result[0].QuestionId.Should().Be("question-1");
        result[1].QuestionId.Should().Be("question-2");
        result[2].QuestionId.Should().Be("question-3");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _postponedQuestionRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoPostponedQuestions_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";

        var query = new GetPostponedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _postponedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PostponedQuestion>());

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
        var timestamp = DateTime.UtcNow;

        var query = new GetPostponedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var postponedQuestions = new List<PostponedQuestion>
        {
            new()
            {
                Id = "postponed-999",
                QuestionId = "question-999",
                Timestamp = timestamp
            }
        };

        _postponedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(postponedQuestions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("postponed-999");
        result[0].QuestionId.Should().Be("question-999");
        result[0].Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "different-random-999";

        var query = new GetPostponedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _postponedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PostponedQuestion>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _postponedQuestionRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidQuery_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var randomizationId = "random-456";

        var query = new GetPostponedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _postponedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PostponedQuestion>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _postponedQuestionRepositoryMock.Verify(
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

        var query = new GetPostponedQuestionsQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _postponedQuestionRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, cancellationToken))
            .ReturnsAsync(new List<PostponedQuestion>());

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();

        _postponedQuestionRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, cancellationToken),
            Times.Once);
    }
}
