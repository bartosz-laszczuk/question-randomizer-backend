namespace QuestionRandomizer.UnitTests.Queries.Qualifications;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Qualifications.GetQualificationById;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetQualificationByIdQueryHandler
/// </summary>
public class GetQualificationByIdQueryHandlerTests
{
    private readonly Mock<IQualificationRepository> _qualificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetQualificationByIdQueryHandler _handler;

    public GetQualificationByIdQueryHandlerTests()
    {
        _qualificationRepositoryMock = new Mock<IQualificationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetQualificationByIdQueryHandler(
            _qualificationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingQualification_ReturnsQualificationDto()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-456";
        var query = new GetQualificationByIdQuery { Id = qualificationId };

        var qualification = new Qualification
        {
            Id = qualificationId,
            Name = "Azure Developer Associate",
            UserId = userId,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualification);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(qualificationId);
        result.Name.Should().Be("Azure Developer Associate");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        result.UpdatedAt.Should().Be(new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc));

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QualificationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "non-existent-qualification";
        var query = new GetQualificationByIdQuery { Id = qualificationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Qualification?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Qualification with ID {qualificationId} not found");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveQualification_ReturnsInactiveQualificationDto()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-inactive";
        var query = new GetQualificationByIdQuery { Id = qualificationId };

        var qualification = new Qualification
        {
            Id = qualificationId,
            Name = "Inactive Qualification",
            UserId = userId,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualification);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DifferentUser_UsesCurrentUserService()
    {
        // Arrange
        var userId1 = "user-1";
        var userId2 = "user-2";
        var qualificationId = "qual-789";
        var query = new GetQualificationByIdQuery { Id = qualificationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId1);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Qualification?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.GetByIdAsync(qualificationId, userId1, It.IsAny<CancellationToken>()),
            Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.GetByIdAsync(qualificationId, userId2, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CancellationToken_IsPassedToRepository()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-999";
        var query = new GetQualificationByIdQuery { Id = qualificationId };
        var cancellationToken = new CancellationToken();

        var qualification = new Qualification
        {
            Id = qualificationId,
            Name = "Test Qualification",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByIdAsync(qualificationId, userId, cancellationToken))
            .ReturnsAsync(qualification);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _qualificationRepositoryMock.Verify(
            x => x.GetByIdAsync(qualificationId, userId, cancellationToken),
            Times.Once);
    }
}
