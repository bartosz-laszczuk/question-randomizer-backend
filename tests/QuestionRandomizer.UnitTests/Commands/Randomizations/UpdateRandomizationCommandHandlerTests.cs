namespace QuestionRandomizer.UnitTests.Commands.Randomizations;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Randomizations.UpdateRandomization;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for UpdateRandomizationCommandHandler
/// </summary>
public class UpdateRandomizationCommandHandlerTests
{
    private readonly Mock<IRandomizationRepository> _randomizationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateRandomizationCommandHandler _handler;

    public UpdateRandomizationCommandHandlerTests()
    {
        _randomizationRepositoryMock = new Mock<IRandomizationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new UpdateRandomizationCommandHandler(
            _randomizationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesRandomizationSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "rand-456";
        var command = new UpdateRandomizationCommand
        {
            Id = randomizationId,
            ShowAnswer = true,
            Status = "Completed",
            CurrentQuestionId = "question-123"
        };

        var existingRandomization = new Randomization
        {
            Id = randomizationId,
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetByIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRandomization);
        _randomizationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(randomizationId);
        result.ShowAnswer.Should().BeTrue();
        result.Status.Should().Be("Completed");
        result.CurrentQuestionId.Should().Be("question-123");

        _randomizationRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Randomization>(r =>
                    r.ShowAnswer == true &&
                    r.Status == "Completed" &&
                    r.CurrentQuestionId == "question-123"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RandomizationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "non-existent-rand";
        var command = new UpdateRandomizationCommand
        {
            Id = randomizationId,
            ShowAnswer = true,
            Status = "Completed",
            CurrentQuestionId = "question-123"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetByIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Randomization?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Randomization with ID {randomizationId} not found");

        _randomizationRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateFails_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "rand-789";
        var command = new UpdateRandomizationCommand
        {
            Id = randomizationId,
            ShowAnswer = true,
            Status = "Completed",
            CurrentQuestionId = "question-456"
        };

        var existingRandomization = new Randomization
        {
            Id = randomizationId,
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetByIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRandomization);
        _randomizationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Randomization with ID {randomizationId} not found");
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "rand-detailed";
        var createdAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var command = new UpdateRandomizationCommand
        {
            Id = randomizationId,
            ShowAnswer = true,
            Status = "Paused",
            CurrentQuestionId = "question-999"
        };

        var existingRandomization = new Randomization
        {
            Id = randomizationId,
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = createdAt
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetByIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRandomization);
        _randomizationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(randomizationId);
        result.UserId.Should().Be(userId);
        result.ShowAnswer.Should().BeTrue();
        result.Status.Should().Be("Paused");
        result.CurrentQuestionId.Should().Be("question-999");
        result.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_UserDoesNotOwnRandomization_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "rand-other-user";
        var command = new UpdateRandomizationCommand
        {
            Id = randomizationId,
            ShowAnswer = true,
            Status = "Completed",
            CurrentQuestionId = "question-123"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetByIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Randomization?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CanClearCurrentQuestion()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "rand-clear";
        var command = new UpdateRandomizationCommand
        {
            Id = randomizationId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null
        };

        var existingRandomization = new Randomization
        {
            Id = randomizationId,
            UserId = userId,
            ShowAnswer = true,
            Status = "Ongoing",
            CurrentQuestionId = "question-to-clear",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetByIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRandomization);
        _randomizationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CurrentQuestionId.Should().BeNull();
    }
}
