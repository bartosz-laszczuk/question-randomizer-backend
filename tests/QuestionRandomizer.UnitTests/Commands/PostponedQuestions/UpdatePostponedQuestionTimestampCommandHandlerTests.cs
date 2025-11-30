namespace QuestionRandomizer.UnitTests.Commands.PostponedQuestions;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.PostponedQuestions.UpdatePostponedQuestionTimestamp;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for UpdatePostponedQuestionTimestampCommandHandler
/// </summary>
public class UpdatePostponedQuestionTimestampCommandHandlerTests
{
    private readonly Mock<IPostponedQuestionRepository> _postponedQuestionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdatePostponedQuestionTimestampCommandHandler _handler;

    public UpdatePostponedQuestionTimestampCommandHandlerTests()
    {
        _postponedQuestionRepositoryMock = new Mock<IPostponedQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new UpdatePostponedQuestionTimestampCommandHandler(
            _postponedQuestionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesPostponedQuestionTimestampSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var questionId = "question-789";

        var command = new UpdatePostponedQuestionTimestampCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _postponedQuestionRepositoryMock
            .Setup(x => x.UpdateTimestampAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _postponedQuestionRepositoryMock.Verify(
            x => x.UpdateTimestampAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "different-random-999";
        var questionId = "question-789";

        var command = new UpdatePostponedQuestionTimestampCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _postponedQuestionRepositoryMock
            .Setup(x => x.UpdateTimestampAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _postponedQuestionRepositoryMock.Verify(
            x => x.UpdateTimestampAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentQuestionId_CallsRepositoryWithCorrectQuestionId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var questionId = "different-question-111";

        var command = new UpdatePostponedQuestionTimestampCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _postponedQuestionRepositoryMock
            .Setup(x => x.UpdateTimestampAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _postponedQuestionRepositoryMock.Verify(
            x => x.UpdateTimestampAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var randomizationId = "random-456";
        var questionId = "question-789";

        var command = new UpdatePostponedQuestionTimestampCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _postponedQuestionRepositoryMock
            .Setup(x => x.UpdateTimestampAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _postponedQuestionRepositoryMock.Verify(
            x => x.UpdateTimestampAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var questionId = "question-789";
        var cancellationToken = new CancellationToken();

        var command = new UpdatePostponedQuestionTimestampCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _postponedQuestionRepositoryMock
            .Setup(x => x.UpdateTimestampAsync(randomizationId, userId, questionId, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(Unit.Value);

        _postponedQuestionRepositoryMock.Verify(
            x => x.UpdateTimestampAsync(randomizationId, userId, questionId, cancellationToken),
            Times.Once);
    }
}
