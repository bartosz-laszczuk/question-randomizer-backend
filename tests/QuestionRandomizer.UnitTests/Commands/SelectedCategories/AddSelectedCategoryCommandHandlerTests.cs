namespace QuestionRandomizer.UnitTests.Commands.SelectedCategories;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.SelectedCategories.AddSelectedCategory;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for AddSelectedCategoryCommandHandler
/// </summary>
public class AddSelectedCategoryCommandHandlerTests
{
    private readonly Mock<ISelectedCategoryRepository> _selectedCategoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AddSelectedCategoryCommandHandler _handler;

    public AddSelectedCategoryCommandHandlerTests()
    {
        _selectedCategoryRepositoryMock = new Mock<ISelectedCategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new AddSelectedCategoryCommandHandler(
            _selectedCategoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsSelectedCategorySuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var categoryId = "category-789";
        var categoryName = "Software Engineering";

        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId,
            CategoryName = categoryName
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategory = new SelectedCategory
        {
            Id = "selected-999",
            CategoryId = categoryId,
            CategoryName = categoryName,
            CreatedAt = DateTime.UtcNow
        };

        _selectedCategoryRepositoryMock
            .Setup(x => x.AddAsync(randomizationId, userId, It.IsAny<SelectedCategory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("selected-999");
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be(categoryName);
        result.CreatedAt.Should().NotBe(default);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _selectedCategoryRepositoryMock.Verify(
            x => x.AddAsync(
                randomizationId,
                userId,
                It.Is<SelectedCategory>(sc =>
                    sc.CategoryId == categoryId &&
                    sc.CategoryName == categoryName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsCreatedAtTimestamp()
    {
        // Arrange
        var userId = "test-user-123";
        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "random-123",
            CategoryId = "category-456",
            CategoryName = "Test Category"
        };

        var beforeCreation = DateTime.UtcNow;

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategory = new SelectedCategory
        {
            Id = "selected-1",
            CategoryId = command.CategoryId,
            CategoryName = command.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        _selectedCategoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SelectedCategory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        var afterCreation = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeAfter(beforeCreation.AddSeconds(-1));
        result.CreatedAt.Should().BeBefore(afterCreation.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "random-123",
            CategoryId = "category-456",
            CategoryName = "Test Category"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategory = new SelectedCategory
        {
            Id = "selected-1",
            CategoryId = command.CategoryId,
            CategoryName = command.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        _selectedCategoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), userId, It.IsAny<SelectedCategory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _selectedCategoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<string>(), userId, It.IsAny<SelectedCategory>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var cancellationToken = new CancellationToken();

        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "random-123",
            CategoryId = "category-456",
            CategoryName = "Test Category"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategory = new SelectedCategory
        {
            Id = "selected-1",
            CategoryId = command.CategoryId,
            CategoryName = command.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        _selectedCategoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SelectedCategory>(), cancellationToken))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();

        _selectedCategoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SelectedCategory>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_PassesCorrectRandomizationId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "unique-random-id-789";

        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = "category-456",
            CategoryName = "Test Category"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var createdCategory = new SelectedCategory
        {
            Id = "selected-1",
            CategoryId = command.CategoryId,
            CategoryName = command.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        _selectedCategoryRepositoryMock
            .Setup(x => x.AddAsync(randomizationId, It.IsAny<string>(), It.IsAny<SelectedCategory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _selectedCategoryRepositoryMock.Verify(
            x => x.AddAsync(randomizationId, It.IsAny<string>(), It.IsAny<SelectedCategory>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
