namespace QuestionRandomizer.UnitTests.Commands.PostponedQuestions;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.PostponedQuestions.AddPostponedQuestion;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for AddPostponedQuestionCommandHandler
/// </summary>
public class AddPostponedQuestionCommandHandlerTests
{
    private readonly Mock<IPostponedQuestionRepository> _postponedQuestionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AddPostponedQuestionCommandHandler _handler;

    public AddPostponedQuestionCommandHandlerTests()
    {
        _postponedQuestionRepositoryMock = new Mock<IPostponedQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new AddPostponedQuestionCommandHandler(
            _postponedQuestionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsPostponedQuestionSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var questionId = "question-789";

        var command = new AddPostponedQuestionCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdPostponedQuestion = new PostponedQuestion
        {
            Id = "postponed-999",
            QuestionId = questionId,
            Timestamp = DateTime.UtcNow
        };

        _postponedQuestionRepositoryMock
            .Setup(x => x.AddAsync(randomizationId, userId, It.IsAny<PostponedQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPostponedQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("postponed-999");
        result.QuestionId.Should().Be(questionId);
        result.Timestamp.Should().NotBe(default);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _postponedQuestionRepositoryMock.Verify(
            x => x.AddAsync(
                randomizationId,
                userId,
                It.Is<PostponedQuestion>(pq => pq.QuestionId == questionId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsTimestamp()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new AddPostponedQuestionCommand
        {
            RandomizationId = "random-123",
            QuestionId = "question-456"
        };

        var beforeCreation = DateTime.UtcNow;

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdPostponedQuestion = new PostponedQuestion
        {
            Id = "postponed-1",
            QuestionId = command.QuestionId,
            Timestamp = DateTime.UtcNow
        };

        _postponedQuestionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PostponedQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPostponedQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        var afterCreation = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.Timestamp.Should().BeAfter(beforeCreation.AddSeconds(-1));
        result.Timestamp.Should().BeBefore(afterCreation.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var command = new AddPostponedQuestionCommand
        {
            RandomizationId = "random-123",
            QuestionId = "question-456"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdPostponedQuestion = new PostponedQuestion
        {
            Id = "postponed-1",
            QuestionId = command.QuestionId,
            Timestamp = DateTime.UtcNow
        };

        _postponedQuestionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), userId, It.IsAny<PostponedQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPostponedQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _postponedQuestionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<string>(), userId, It.IsAny<PostponedQuestion>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var cancellationToken = new CancellationToken();

        var command = new AddPostponedQuestionCommand
        {
            RandomizationId = "random-123",
            QuestionId = "question-456"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdPostponedQuestion = new PostponedQuestion
        {
            Id = "postponed-1",
            QuestionId = command.QuestionId,
            Timestamp = DateTime.UtcNow
        };

        _postponedQuestionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PostponedQuestion>(), cancellationToken))
            .ReturnsAsync(createdPostponedQuestion);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();

        _postponedQuestionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PostponedQuestion>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_PassesCorrectRandomizationId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "unique-random-id-789";

        var command = new AddPostponedQuestionCommand
        {
            RandomizationId = randomizationId,
            QuestionId = "question-456"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdPostponedQuestion = new PostponedQuestion
        {
            Id = "postponed-1",
            QuestionId = command.QuestionId,
            Timestamp = DateTime.UtcNow
        };

        _postponedQuestionRepositoryMock
            .Setup(x => x.AddAsync(randomizationId, It.IsAny<string>(), It.IsAny<PostponedQuestion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPostponedQuestion);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _postponedQuestionRepositoryMock.Verify(
            x => x.AddAsync(randomizationId, It.IsAny<string>(), It.IsAny<PostponedQuestion>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
