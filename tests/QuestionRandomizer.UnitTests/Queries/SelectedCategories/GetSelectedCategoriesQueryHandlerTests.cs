namespace QuestionRandomizer.UnitTests.Queries.SelectedCategories;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.SelectedCategories.GetSelectedCategories;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetSelectedCategoriesQueryHandler
/// </summary>
public class GetSelectedCategoriesQueryHandlerTests
{
    private readonly Mock<ISelectedCategoryRepository> _selectedCategoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetSelectedCategoriesQueryHandler _handler;

    public GetSelectedCategoriesQueryHandlerTests()
    {
        _selectedCategoryRepositoryMock = new Mock<ISelectedCategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetSelectedCategoriesQueryHandler(
            _selectedCategoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsSelectedCategories()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";

        var query = new GetSelectedCategoriesQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var selectedCategories = new List<SelectedCategory>
        {
            new()
            {
                Id = "selected-1",
                CategoryId = "category-1",
                CategoryName = "Software Engineering",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "selected-2",
                CategoryId = "category-2",
                CategoryName = "Cloud Computing",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "selected-3",
                CategoryId = "category-3",
                CategoryName = "Database Design",
                CreatedAt = DateTime.UtcNow
            }
        };

        _selectedCategoryRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(selectedCategories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Id.Should().Be("selected-1");
        result[0].CategoryName.Should().Be("Software Engineering");
        result[1].CategoryName.Should().Be("Cloud Computing");
        result[2].CategoryName.Should().Be("Database Design");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _selectedCategoryRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoSelectedCategories_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";

        var query = new GetSelectedCategoriesQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _selectedCategoryRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SelectedCategory>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsMappedDtos()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var now = DateTime.UtcNow;

        var query = new GetSelectedCategoriesQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var selectedCategories = new List<SelectedCategory>
        {
            new()
            {
                Id = "selected-999",
                CategoryId = "category-999",
                CategoryName = "Test Category",
                CreatedAt = now
            }
        };

        _selectedCategoryRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(selectedCategories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("selected-999");
        result[0].CategoryId.Should().Be("category-999");
        result[0].CategoryName.Should().Be("Test Category");
        result[0].CreatedAt.Should().Be(now);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "different-random-999";

        var query = new GetSelectedCategoriesQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _selectedCategoryRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SelectedCategory>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _selectedCategoryRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidQuery_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var randomizationId = "random-456";

        var query = new GetSelectedCategoriesQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _selectedCategoryRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SelectedCategory>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _selectedCategoryRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidQuery_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var cancellationToken = new CancellationToken();

        var query = new GetSelectedCategoriesQuery
        {
            RandomizationId = randomizationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _selectedCategoryRepositoryMock
            .Setup(x => x.GetByRandomizationIdAsync(randomizationId, userId, cancellationToken))
            .ReturnsAsync(new List<SelectedCategory>());

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();

        _selectedCategoryRepositoryMock.Verify(
            x => x.GetByRandomizationIdAsync(randomizationId, userId, cancellationToken),
            Times.Once);
    }
}
