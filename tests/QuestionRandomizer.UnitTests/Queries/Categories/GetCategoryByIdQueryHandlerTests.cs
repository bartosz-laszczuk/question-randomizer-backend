namespace QuestionRandomizer.UnitTests.Queries.Categories;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Categories.GetCategoryById;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetCategoryByIdQueryHandler
/// </summary>
public class GetCategoryByIdQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetCategoryByIdQueryHandler _handler;

    public GetCategoryByIdQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetCategoryByIdQueryHandler(
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingCategory_ReturnsCategoryDto()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";
        var query = new GetCategoryByIdQuery { Id = categoryId };

        var category = new Category
        {
            Id = categoryId,
            Name = "Software Engineering",
            UserId = userId,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(categoryId);
        result.Name.Should().Be("Software Engineering");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        result.UpdatedAt.Should().Be(new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc));

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "non-existent-category";
        var query = new GetCategoryByIdQuery { Id = categoryId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveCategory_ReturnsInactiveCategoryDto()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-inactive";
        var query = new GetCategoryByIdQuery { Id = categoryId };

        var category = new Category
        {
            Id = categoryId,
            Name = "Inactive Category",
            UserId = userId,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

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
        var categoryId = "category-789";
        var query = new GetCategoryByIdQuery { Id = categoryId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId1);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(categoryId, userId1, It.IsAny<CancellationToken>()),
            Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(categoryId, userId2, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CancellationToken_IsPassedToRepository()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-999";
        var query = new GetCategoryByIdQuery { Id = categoryId };
        var cancellationToken = new CancellationToken();

        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, userId, cancellationToken))
            .ReturnsAsync(category);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(categoryId, userId, cancellationToken),
            Times.Once);
    }
}
