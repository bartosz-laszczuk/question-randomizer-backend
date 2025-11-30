namespace QuestionRandomizer.UnitTests.Queries.Categories;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Categories.GetCategories;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetCategoriesQueryHandler
/// </summary>
public class GetCategoriesQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetCategoriesQueryHandler(
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllUserCategories()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetCategoriesQuery();

        var categories = new List<Category>
        {
            new Category
            {
                Id = "cat1",
                Name = "Software Engineering",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Category
            {
                Id = "cat2",
                Name = "System Design",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("cat1");
        result[0].Name.Should().Be("Software Engineering");
        result[1].Id.Should().Be("cat2");
        result[1].Name.Should().Be("System Design");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoCategoriesFound_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetCategoriesQuery();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

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
        var query = new GetCategoriesQuery();
        var createdAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc);

        var categories = new List<Category>
        {
            new Category
            {
                Id = "cat-detailed",
                Name = "Detailed Category",
                UserId = userId,
                IsActive = true,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be("cat-detailed");
        dto.Name.Should().Be("Detailed Category");
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task Handle_IncludesInactiveCategories()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetCategoriesQuery();

        var categories = new List<Category>
        {
            new Category
            {
                Id = "cat-active",
                Name = "Active Category",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Category
            {
                Id = "cat-inactive",
                Name = "Inactive Category",
                UserId = userId,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == "cat-active" && c.IsActive);
        result.Should().Contain(c => c.Id == "cat-inactive" && !c.IsActive);
    }

    [Fact]
    public async Task Handle_UsesCurrentUserService()
    {
        // Arrange
        var userId = "specific-user-999";
        var query = new GetCategoriesQuery();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CancellationToken_IsPassedToRepository()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetCategoriesQuery();
        var cancellationToken = new CancellationToken();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, cancellationToken))
            .ReturnsAsync(new List<Category>());

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, cancellationToken),
            Times.Once);
    }
}
