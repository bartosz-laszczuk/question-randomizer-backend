namespace QuestionRandomizer.UnitTests.Queries.Qualifications;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Qualifications.GetQualifications;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetQualificationsQueryHandler
/// </summary>
public class GetQualificationsQueryHandlerTests
{
    private readonly Mock<IQualificationRepository> _qualificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetQualificationsQueryHandler _handler;

    public GetQualificationsQueryHandlerTests()
    {
        _qualificationRepositoryMock = new Mock<IQualificationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetQualificationsQueryHandler(
            _qualificationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllUserQualifications()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQualificationsQuery();

        var qualifications = new List<Qualification>
        {
            new Qualification
            {
                Id = "qual1",
                Name = "Azure Developer",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Qualification
            {
                Id = "qual2",
                Name = "AWS Architect",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("qual1");
        result[0].Name.Should().Be("Azure Developer");
        result[1].Id.Should().Be("qual2");
        result[1].Name.Should().Be("AWS Architect");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoQualificationsFound_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQualificationsQuery();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Qualification>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQualificationsQuery();
        var createdAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc);

        var qualifications = new List<Qualification>
        {
            new Qualification
            {
                Id = "qual-detailed",
                Name = "Detailed Qualification",
                UserId = userId,
                IsActive = true,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be("qual-detailed");
        dto.Name.Should().Be("Detailed Qualification");
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task Handle_IncludesInactiveQualifications()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQualificationsQuery();

        var qualifications = new List<Qualification>
        {
            new Qualification
            {
                Id = "qual-active",
                Name = "Active Qualification",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Qualification
            {
                Id = "qual-inactive",
                Name = "Inactive Qualification",
                UserId = userId,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(q => q.Id == "qual-active" && q.IsActive);
        result.Should().Contain(q => q.Id == "qual-inactive" && !q.IsActive);
    }

    [Fact]
    public async Task Handle_UsesCurrentUserService()
    {
        // Arrange
        var userId = "specific-user-999";
        var query = new GetQualificationsQuery();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Qualification>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CancellationToken_IsPassedToRepository()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetQualificationsQuery();
        var cancellationToken = new CancellationToken();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _qualificationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, cancellationToken))
            .ReturnsAsync(new List<Qualification>());

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _qualificationRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, cancellationToken),
            Times.Once);
    }
}
