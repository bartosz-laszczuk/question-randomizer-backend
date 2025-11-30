namespace QuestionRandomizer.UnitTests.Commands.Randomizations;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Randomizations.CreateRandomization;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for CreateRandomizationCommandHandler
/// </summary>
public class CreateRandomizationCommandHandlerTests
{
    private readonly Mock<IRandomizationRepository> _randomizationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateRandomizationCommandHandler _handler;

    public CreateRandomizationCommandHandlerTests()
    {
        _randomizationRepositoryMock = new Mock<IRandomizationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateRandomizationCommandHandler(
            _randomizationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesRandomizationSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateRandomizationCommand();

        var createdRandomization = new Randomization
        {
            Id = "rand-456",
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRandomization);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("rand-456");
        result.UserId.Should().Be(userId);
        result.ShowAnswer.Should().BeFalse();
        result.Status.Should().Be("Ongoing");
        result.CurrentQuestionId.Should().BeNull();
        result.CreatedAt.Should().NotBeNull();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _randomizationRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Randomization>(r =>
                    r.UserId == userId &&
                    r.ShowAnswer == false &&
                    r.Status == "Ongoing" &&
                    r.CurrentQuestionId == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SetsDefaultValues()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateRandomizationCommand();

        var createdRandomization = new Randomization
        {
            Id = "rand-789",
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRandomization);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShowAnswer.Should().BeFalse();
        result.Status.Should().Be("Ongoing");
        result.CurrentQuestionId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SetsTimestampToUtcNow()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateRandomizationCommand();
        var beforeCreate = DateTime.UtcNow;

        var createdRandomization = new Randomization
        {
            Id = "rand-999",
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRandomization);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().NotBeNull();
        result.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        result.CreatedAt.Should().BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task Handle_UsesCurrentUserService()
    {
        // Arrange
        var userId = "specific-user-999";
        var command = new CreateRandomizationCommand();

        var createdRandomization = new Randomization
        {
            Id = "rand-111",
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRandomization);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _randomizationRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Randomization>(r => r.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateRandomizationCommand();
        var createdAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var createdRandomization = new Randomization
        {
            Id = "rand-detailed",
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = createdAt
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Randomization>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRandomization);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be("rand-detailed");
        result.UserId.Should().Be(userId);
        result.ShowAnswer.Should().BeFalse();
        result.Status.Should().Be("Ongoing");
        result.CurrentQuestionId.Should().BeNull();
        result.CreatedAt.Should().Be(createdAt);
    }
}
