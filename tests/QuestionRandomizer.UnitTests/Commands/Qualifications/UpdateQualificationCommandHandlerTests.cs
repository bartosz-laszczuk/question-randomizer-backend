namespace QuestionRandomizer.UnitTests.Commands.Qualifications;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Qualifications.UpdateQualification;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for UpdateQualificationCommandHandler
/// </summary>
public class UpdateQualificationCommandHandlerTests
{
    private readonly Mock<IQualificationRepository> _qualificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateQualificationCommandHandler _handler;

    public UpdateQualificationCommandHandlerTests()
    {
        _qualificationRepositoryMock = new Mock<IQualificationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new UpdateQualificationCommandHandler(
            _qualificationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesQualificationSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-456";
        var command = new UpdateQualificationCommand
        {
            Id = qualificationId,
            Name = "Updated Azure Developer"
        };

        var existingQualification = new Qualification
        {
            Id = qualificationId,
            Name = "Old Azure Developer",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQualification);
        _qualificationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(qualificationId);
        result.Name.Should().Be("Updated Azure Developer");
        result.IsActive.Should().BeTrue();

        _qualificationRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Qualification>(q => q.Name == command.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QualificationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "non-existent-qualification";
        var command = new UpdateQualificationCommand
        {
            Id = qualificationId,
            Name = "Updated Name"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Qualification?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Qualification with ID {qualificationId} not found");

        _qualificationRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateFails_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-789";
        var command = new UpdateQualificationCommand
        {
            Id = qualificationId,
            Name = "Updated Name"
        };

        var existingQualification = new Qualification
        {
            Id = qualificationId,
            Name = "Old Name",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQualification);
        _qualificationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Qualification with ID {qualificationId} not found");
    }

    [Fact]
    public async Task Handle_UpdatesTimestamp()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-999";
        var command = new UpdateQualificationCommand
        {
            Id = qualificationId,
            Name = "New Name"
        };

        var beforeUpdate = DateTime.UtcNow;
        var existingQualification = new Qualification
        {
            Id = qualificationId,
            Name = "Old Name",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQualification);
        _qualificationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Qualification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterUpdate = DateTime.UtcNow;

        // Assert
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
        result.UpdatedAt.Should().BeOnOrBefore(afterUpdate);
    }
}
