namespace QuestionRandomizer.UnitTests.Queries.Randomizations;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Randomizations.GetRandomization;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetRandomizationQueryHandler
/// </summary>
public class GetRandomizationQueryHandlerTests
{
    private readonly Mock<IRandomizationRepository> _randomizationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetRandomizationQueryHandler _handler;

    public GetRandomizationQueryHandlerTests()
    {
        _randomizationRepositoryMock = new Mock<IRandomizationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetRandomizationQueryHandler(
            _randomizationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ActiveRandomizationExists_ReturnsRandomizationDto()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetRandomizationQuery();

        var randomization = new Randomization
        {
            Id = "rand-456",
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = "question-123",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(randomization);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("rand-456");
        result.UserId.Should().Be(userId);
        result.ShowAnswer.Should().BeFalse();
        result.Status.Should().Be("Ongoing");
        result.CurrentQuestionId.Should().Be("question-123");
        result.CreatedAt.Should().NotBeNull();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _randomizationRepositoryMock.Verify(
            x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoActiveRandomization_ReturnsNull()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetRandomizationQuery();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Randomization?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetRandomizationQuery();
        var createdAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var randomization = new Randomization
        {
            Id = "rand-detailed",
            UserId = userId,
            ShowAnswer = true,
            Status = "Paused",
            CurrentQuestionId = "question-999",
            CreatedAt = createdAt
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(randomization);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("rand-detailed");
        result.UserId.Should().Be(userId);
        result.ShowAnswer.Should().BeTrue();
        result.Status.Should().Be("Paused");
        result.CurrentQuestionId.Should().Be("question-999");
        result.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_RandomizationWithoutCurrentQuestion_MapsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetRandomizationQuery();

        var randomization = new Randomization
        {
            Id = "rand-no-question",
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _randomizationRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(randomization);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentQuestionId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UsesCurrentUserService()
    {
        // Arrange
        var userId = "specific-user-999";
        var query = new GetRandomizationQuery();

        var randomization = new Randomization
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
            .Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(randomization);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _randomizationRepositoryMock.Verify(
            x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
