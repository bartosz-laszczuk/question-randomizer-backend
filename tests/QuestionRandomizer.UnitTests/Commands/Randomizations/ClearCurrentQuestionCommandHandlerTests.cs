namespace QuestionRandomizer.UnitTests.Commands.Randomizations;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.Randomizations.ClearCurrentQuestion;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for ClearCurrentQuestionCommandHandler
/// </summary>
public class ClearCurrentQuestionCommandHandlerTests
{
    private readonly Mock<IRandomizationRepository> _randomizationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ClearCurrentQuestionCommandHandler _handler;

    public ClearCurrentQuestionCommandHandlerTests()
    {
        _randomizationRepositoryMock = new Mock<IRandomizationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new ClearCurrentQuestionCommandHandler(
            _randomizationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingRandomization_ClearsCurrentQuestionSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "rand-456";
        var command = new ClearCurrentQuestionCommand { RandomizationId = randomizationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.ClearCurrentQuestionAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _randomizationRepositoryMock.Verify(
            x => x.ClearCurrentQuestionAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRandomization_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "non-existent-rand";
        var command = new ClearCurrentQuestionCommand { RandomizationId = randomizationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.ClearCurrentQuestionAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Randomization with ID {randomizationId} not found");

        _randomizationRepositoryMock.Verify(
            x => x.ClearCurrentQuestionAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsUnitValue()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "rand-789";
        var command = new ClearCurrentQuestionCommand { RandomizationId = randomizationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.ClearCurrentQuestionAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Unit>();
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_UserDoesNotOwnRandomization_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "rand-other-user";
        var command = new ClearCurrentQuestionCommand { RandomizationId = randomizationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.ClearCurrentQuestionAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
