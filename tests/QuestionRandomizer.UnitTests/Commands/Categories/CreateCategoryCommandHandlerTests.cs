namespace QuestionRandomizer.UnitTests.Commands.Categories;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Categories.CreateCategory;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for CreateCategoryCommandHandler
/// </summary>
public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesCategorySuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateCategoryCommand { Name = "Software Engineering" };

        var createdCategory = new Category
        {
            Id = "category-456",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("category-456");
        result.Name.Should().Be(command.Name);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().NotBeNull();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Category>(c =>
                    c.Name == command.Name &&
                    c.UserId == userId &&
                    c.IsActive == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SetsTimestampsToUtcNow()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateCategoryCommand { Name = "Test Category" };
        var beforeCreate = DateTime.UtcNow;

        var createdCategory = new Category
        {
            Id = "category-789",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

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
        var command = new CreateCategoryCommand { Name = "User Category" };

        var createdCategory = new Category
        {
            Id = "category-111",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Category>(c => c.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DefaultsIsActiveToTrue()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new CreateCategoryCommand { Name = "Active Category" };

        var createdCategory = new Category
        {
            Id = "category-222",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

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
        var command = new CreateCategoryCommand { Name = "Test Category" };
        var cancellationToken = new CancellationToken();

        var createdCategory = new Category
        {
            Id = "category-333",
            Name = command.Name,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Category>(), cancellationToken))
            .ReturnsAsync(createdCategory);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Category>(), cancellationToken),
            Times.Once);
    }
}
