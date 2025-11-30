namespace QuestionRandomizer.UnitTests.Commands.Categories;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Categories.UpdateCategory;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for UpdateCategoryCommandHandler
/// </summary>
public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new UpdateCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesCategorySuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";
        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "Updated Category Name"
        };

        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Old Category Name",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);
        _categoryRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(categoryId);
        result.Name.Should().Be("Updated Category Name");
        result.IsActive.Should().BeTrue();

        _categoryRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Category>(c => c.Name == command.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "non-existent-category";
        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "Updated Name"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found");

        _categoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateFails_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-789";
        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "Updated Name"
        };

        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Old Name",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);
        _categoryRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found");
    }

    [Fact]
    public async Task Handle_UpdatesTimestamp()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-999";
        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "New Name"
        };

        var beforeUpdate = DateTime.UtcNow;
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Old Name",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);
        _categoryRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
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
