namespace QuestionRandomizer.UnitTests.Commands.Qualifications;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualification;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for CreateQualificationCommandHandler
/// </summary>
public class CreateQualificationCommandHandlerTests
{
    private readonly Mock<IQualificationRepository> _qualificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateQualificationCommandHandler _handler;

    public CreateQualificationCommandHandlerTests()
    {
        _qualificationRepositoryMock = new Mock<IQualificationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateQualificationCommandHandler(
            _qualificationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesQualificationSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQualificationCommand { Name = "Azure Developer Associate" };

        var createdQualification = new Qualification
        {
            Id = "qual-456",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualification);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("qual-456");
        result.Name.Should().Be(command.Name);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().NotBeNull();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Qualification>(q =>
                    q.Name == command.Name &&
                    q.UserId == userId &&
                    q.IsActive == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SetsTimestampsToUtcNow()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQualificationCommand { Name = "AWS Solutions Architect" };
        var beforeCreate = DateTime.UtcNow;

        var createdQualification = new Qualification
        {
            Id = "qual-789",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualification);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().NotBeNull();
        result.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        result.CreatedAt.Should().BeOnOrBefore(afterCreate);
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeOnOrAfter(beforeCreate);
        result.UpdatedAt.Should().BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task Handle_UsesCurrentUserService()
    {
        // Arrange
        var userId = "specific-user-999";
        var command = new CreateQualificationCommand { Name = "Google Cloud Professional" };

        var createdQualification = new Qualification
        {
            Id = "qual-111",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualification);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Qualification>(q => q.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DefaultsIsActiveToTrue()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQualificationCommand { Name = "Active Qualification" };

        var createdQualification = new Qualification
        {
            Id = "qual-222",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualification);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CancellationToken_IsPassedToRepository()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQualificationCommand { Name = "Test Qualification" };
        var cancellationToken = new CancellationToken();

        var createdQualification = new Qualification
        {
            Id = "qual-333",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Qualification>(), cancellationToken))
            .ReturnsAsync(createdQualification);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _qualificationRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Qualification>(), cancellationToken),
            Times.Once);
    }
}
