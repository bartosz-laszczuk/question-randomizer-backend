namespace QuestionRandomizer.UnitTests.Commands.Categories;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Categories.CreateCategoriesBatch;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for CreateCategoriesBatchCommandHandler
/// </summary>
public class CreateCategoriesBatchCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateCategoriesBatchCommandHandler _handler;

    public CreateCategoriesBatchCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateCategoriesBatchCommandHandler(
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidBatchCommand_CreatesMultipleCategoriesSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string>
            {
                "Software Engineering",
                "Cloud Computing",
                "Database Design"
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategories = new List<Category>
        {
            new()
            {
                Id = "category-1",
                Name = "Software Engineering",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "category-2",
                Name = "Cloud Computing",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "category-3",
                Name = "Database Design",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _categoryRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategories);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Software Engineering");
        result[1].Name.Should().Be("Cloud Computing");
        result[2].Name.Should().Be("Database Design");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Category>>(c =>
                    c.Count == 3 &&
                    c.All(cat => cat.UserId == userId) &&
                    c[0].Name == "Software Engineering" &&
                    c[1].Name == "Cloud Computing" &&
                    c[2].Name == "Database Design"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SingleCategory_CreatesOneCategory()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string> { "DevOps" }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategories = new List<Category>
        {
            new()
            {
                Id = "category-1",
                Name = "DevOps",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _categoryRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategories);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("DevOps");
        result[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmptyCategoryList_CreatesNoCategories()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string>()
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _categoryRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _categoryRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Category>>(c => c.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsCorrectUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string> { "Category A", "Category B" }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategories = new List<Category>
        {
            new()
            {
                Id = "category-1",
                Name = "Category A",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "category-2",
                Name = "Category B",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _categoryRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategories);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _categoryRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Category>>(c => c.All(cat => cat.UserId == userId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCorrectDtoFields()
    {
        // Arrange
        var userId = "test-user-123";
        var now = DateTime.UtcNow;

        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string> { "Test Category" }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategories = new List<Category>
        {
            new()
            {
                Id = "category-999",
                Name = "Test Category",
                UserId = userId,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        _categoryRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategories);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("category-999");
        result[0].Name.Should().Be("Test Category");
        result[0].IsActive.Should().BeTrue();
        result[0].CreatedAt.Should().Be(now);
        result[0].UpdatedAt.Should().Be(now);
    }

    [Fact]
    public async Task Handle_LargeBatch_CreatesAllCategories()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryNames = Enumerable.Range(1, 100).Select(i => $"Category {i}").ToList();

        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = categoryNames
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategories = categoryNames.Select((name, index) => new Category
        {
            Id = $"category-{index + 1}",
            Name = name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        _categoryRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategories);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(100);
        result.Should().AllSatisfy(c => c.IsActive.Should().BeTrue());

        _categoryRepositoryMock.Verify(
            x => x.CreateManyAsync(
                It.Is<List<Category>>(c => c.Count == 100),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var cancellationToken = new CancellationToken();

        var command = new CreateCategoriesBatchCommand
        {
            CategoryNames = new List<string> { "Test Category" }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategories = new List<Category>
        {
            new()
            {
                Id = "category-1",
                Name = "Test Category",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _categoryRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<List<Category>>(), cancellationToken))
            .ReturnsAsync(createdCategories);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();

        _categoryRepositoryMock.Verify(
            x => x.CreateManyAsync(It.IsAny<List<Category>>(), cancellationToken),
            Times.Once);
    }
}
