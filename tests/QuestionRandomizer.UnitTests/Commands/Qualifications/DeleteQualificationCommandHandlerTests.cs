namespace QuestionRandomizer.UnitTests.Commands.Qualifications;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.Qualifications.DeleteQualification;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for DeleteQualificationCommandHandler
/// </summary>
public class DeleteQualificationCommandHandlerTests
{
    private readonly Mock<IQualificationRepository> _qualificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteQualificationCommandHandler _handler;

    public DeleteQualificationCommandHandlerTests()
    {
        _qualificationRepositoryMock = new Mock<IQualificationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new DeleteQualificationCommandHandler(
            _qualificationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingQualification_DeletesSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-456";
        var command = new DeleteQualificationCommand { Id = qualificationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.DeleteAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.DeleteAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentQualification_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "non-existent-qualification";
        var command = new DeleteQualificationCommand { Id = qualificationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.DeleteAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Qualification with ID {qualificationId} not found");

        _qualificationRepositoryMock.Verify(
            x => x.DeleteAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsUnitValue()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-789";
        var command = new DeleteQualificationCommand { Id = qualificationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.DeleteAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Unit>();
        result.Should().Be(Unit.Value);
    }
}
