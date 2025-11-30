namespace QuestionRandomizer.UnitTests.Commands.UsedQuestions;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.UsedQuestions.DeleteUsedQuestion;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for DeleteUsedQuestionCommandHandler
/// </summary>
public class DeleteUsedQuestionCommandHandlerTests
{
    private readonly Mock<IUsedQuestionRepository> _usedQuestionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteUsedQuestionCommandHandler _handler;

    public DeleteUsedQuestionCommandHandlerTests()
    {
        _usedQuestionRepositoryMock = new Mock<IUsedQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new DeleteUsedQuestionCommandHandler(
            _usedQuestionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesUsedQuestionSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var questionId = "question-789";

        var command = new DeleteUsedQuestionCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _usedQuestionRepositoryMock.Verify(
            x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "different-random-999";
        var questionId = "question-789";

        var command = new DeleteUsedQuestionCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _usedQuestionRepositoryMock.Verify(
            x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentQuestionId_CallsRepositoryWithCorrectQuestionId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var questionId = "different-question-111";

        var command = new DeleteUsedQuestionCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _usedQuestionRepositoryMock.Verify(
            x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var randomizationId = "random-456";
        var questionId = "question-789";

        var command = new DeleteUsedQuestionCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _usedQuestionRepositoryMock.Verify(
            x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, It.IsAny<CancellationToken>()),
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

        var command = new DeleteUsedQuestionCommand
        {
            RandomizationId = randomizationId,
            QuestionId = questionId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(Unit.Value);

        _usedQuestionRepositoryMock.Verify(
            x => x.DeleteByQuestionIdAsync(randomizationId, userId, questionId, cancellationToken),
            Times.Once);
    }
}
