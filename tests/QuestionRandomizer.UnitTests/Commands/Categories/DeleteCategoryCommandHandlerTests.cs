namespace QuestionRandomizer.UnitTests.Commands.Categories;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.Categories.DeleteCategory;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for DeleteCategoryCommandHandler
/// </summary>
public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new DeleteCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingCategory_DeletesSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";
        var command = new DeleteCategoryCommand { Id = categoryId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.DeleteAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _categoryRepositoryMock.Verify(
            x => x.DeleteAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "non-existent-category";
        var command = new DeleteCategoryCommand { Id = categoryId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.DeleteAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found");

        _categoryRepositoryMock.Verify(
            x => x.DeleteAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsUnitValue()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-789";
        var command = new DeleteCategoryCommand { Id = categoryId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _categoryRepositoryMock
            .Setup(x => x.DeleteAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Unit>();
        result.Should().Be(Unit.Value);
    }
}
