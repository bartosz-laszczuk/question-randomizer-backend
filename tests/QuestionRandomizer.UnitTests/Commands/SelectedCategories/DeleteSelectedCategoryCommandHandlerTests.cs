namespace QuestionRandomizer.UnitTests.Commands.SelectedCategories;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.SelectedCategories.DeleteSelectedCategory;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for DeleteSelectedCategoryCommandHandler
/// </summary>
public class DeleteSelectedCategoryCommandHandlerTests
{
    private readonly Mock<ISelectedCategoryRepository> _selectedCategoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteSelectedCategoryCommandHandler _handler;

    public DeleteSelectedCategoryCommandHandlerTests()
    {
        _selectedCategoryRepositoryMock = new Mock<ISelectedCategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new DeleteSelectedCategoryCommandHandler(
            _selectedCategoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesSelectedCategorySuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var categoryId = "category-789";

        var command = new DeleteSelectedCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _selectedCategoryRepositoryMock
            .Setup(x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _selectedCategoryRepositoryMock.Verify(
            x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "different-random-999";
        var categoryId = "category-789";

        var command = new DeleteSelectedCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _selectedCategoryRepositoryMock
            .Setup(x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _selectedCategoryRepositoryMock.Verify(
            x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentCategoryId_CallsRepositoryWithCorrectCategoryId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var categoryId = "different-category-111";

        var command = new DeleteSelectedCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _selectedCategoryRepositoryMock
            .Setup(x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _selectedCategoryRepositoryMock.Verify(
            x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var randomizationId = "random-456";
        var categoryId = "category-789";

        var command = new DeleteSelectedCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _selectedCategoryRepositoryMock
            .Setup(x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _selectedCategoryRepositoryMock.Verify(
            x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var categoryId = "category-789";
        var cancellationToken = new CancellationToken();

        var command = new DeleteSelectedCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _selectedCategoryRepositoryMock
            .Setup(x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(Unit.Value);

        _selectedCategoryRepositoryMock.Verify(
            x => x.DeleteByCategoryIdAsync(randomizationId, userId, categoryId, cancellationToken),
            Times.Once);
    }
}
