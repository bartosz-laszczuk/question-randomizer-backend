namespace QuestionRandomizer.UnitTests.Commands.Qualifications;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualificationsBatch;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for CreateQualificationsBatchCommandHandler
/// </summary>
public class CreateQualificationsBatchCommandHandlerTests
{
    private readonly Mock<IQualificationRepository> _qualificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateQualificationsBatchCommandHandler _handler;

    public CreateQualificationsBatchCommandHandlerTests()
    {
        _qualificationRepositoryMock = new Mock<IQualificationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateQualificationsBatchCommandHandler(
            _qualificationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidBatchCommand_CreatesMultipleQualificationsSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string>
            {
                "Azure Developer Associate",
                "AWS Solutions Architect",
                "Google Cloud Professional"
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQualifications = new List<Qualification>
        {
            new()
            {
                Id = "qual-1",
                Name = "Azure Developer Associate",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "qual-2",
                Name = "AWS Solutions Architect",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "qual-3",
                Name = "Google Cloud Professional",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _qualificationRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Qualification>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualifications);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Azure Developer Associate");
        result[1].Name.Should().Be("AWS Solutions Architect");
        result[2].Name.Should().Be("Google Cloud Professional");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _qualificationRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Qualification>>(q =>
                    q.Count == 3 &&
                    q.All(qual => qual.UserId == userId) &&
                    q[0].Name == "Azure Developer Associate" &&
                    q[1].Name == "AWS Solutions Architect" &&
                    q[2].Name == "Google Cloud Professional"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SingleQualification_CreatesOneQualification()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string> { "Kubernetes Certified" }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQualifications = new List<Qualification>
        {
            new()
            {
                Id = "qual-1",
                Name = "Kubernetes Certified",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _qualificationRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Qualification>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualifications);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Kubernetes Certified");
        result[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmptyQualificationList_CreatesNoQualifications()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string>()
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _qualificationRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Qualification>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Qualification>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _qualificationRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Qualification>>(q => q.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsCorrectUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string> { "Qualification A", "Qualification B" }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQualifications = new List<Qualification>
        {
            new()
            {
                Id = "qual-1",
                Name = "Qualification A",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "qual-2",
                Name = "Qualification B",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _qualificationRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Qualification>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualifications);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _qualificationRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Qualification>>(q => q.All(qual => qual.UserId == userId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCorrectDtoFields()
    {
        // Arrange
        var userId = "test-user-123";
        var now = DateTime.UtcNow;

        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string> { "Test Qualification" }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQualifications = new List<Qualification>
        {
            new()
            {
                Id = "qual-999",
                Name = "Test Qualification",
                UserId = userId,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        _qualificationRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Qualification>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualifications);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("qual-999");
        result[0].Name.Should().Be("Test Qualification");
        result[0].IsActive.Should().BeTrue();
        result[0].CreatedAt.Should().Be(now);
        result[0].UpdatedAt.Should().Be(now);
    }

    [Fact]
    public async Task Handle_LargeBatch_CreatesAllQualifications()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationNames = Enumerable.Range(1, 50).Select(i => $"Qualification {i}").ToList();

        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = qualificationNames
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQualifications = qualificationNames.Select((name, index) => new Qualification
        {
            Id = $"qual-{index + 1}",
            Name = name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        _qualificationRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Qualification>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdQualifications);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(50);
        result.Should().AllSatisfy(q => q.IsActive.Should().BeTrue());

        _qualificationRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Qualification>>(q => q.Count == 50),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var cancellationToken = new CancellationToken();

        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string> { "Test Qualification" }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdQualifications = new List<Qualification>
        {
            new()
            {
                Id = "qual-1",
                Name = "Test Qualification",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _qualificationRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Qualification>>(), cancellationToken))
            .ReturnsAsync(createdQualifications);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();

        _qualificationRepositoryMock.Verify(
            x => x.CreateManyAsync(It.IsAny<List<Qualification>>(), cancellationToken),
            Times.Once);
    }
}
