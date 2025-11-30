namespace QuestionRandomizer.UnitTests.Commands.UsedQuestions;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.UsedQuestions.UpdateUsedQuestionCategory;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for UpdateUsedQuestionCategoryCommandHandler
/// </summary>
public class UpdateUsedQuestionCategoryCommandHandlerTests
{
    private readonly Mock<IUsedQuestionRepository> _usedQuestionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateUsedQuestionCategoryCommandHandler _handler;

    public UpdateUsedQuestionCategoryCommandHandlerTests()
    {
        _usedQuestionRepositoryMock = new Mock<IUsedQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new UpdateUsedQuestionCategoryCommandHandler(
            _usedQuestionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesUsedQuestionCategorySuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var categoryId = "category-789";
        var categoryName = "Updated Category";

        var command = new UpdateUsedQuestionCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId,
            CategoryName = categoryName
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _usedQuestionRepositoryMock.Verify(
            x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentRandomizationId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "different-random-999";
        var categoryId = "category-789";
        var categoryName = "Updated Category";

        var command = new UpdateUsedQuestionCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId,
            CategoryName = categoryName
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _usedQuestionRepositoryMock.Verify(
            x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentCategoryName_CallsRepositoryWithCorrectName()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var categoryId = "category-789";
        var categoryName = "Completely New Category Name";

        var command = new UpdateUsedQuestionCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId,
            CategoryName = categoryName
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _usedQuestionRepositoryMock.Verify(
            x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var randomizationId = "random-456";
        var categoryId = "category-789";
        var categoryName = "Updated Category";

        var command = new UpdateUsedQuestionCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId,
            CategoryName = categoryName
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _usedQuestionRepositoryMock.Verify(
            x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var randomizationId = "random-456";
        var categoryId = "category-789";
        var categoryName = "Updated Category";
        var cancellationToken = new CancellationToken();

        var command = new UpdateUsedQuestionCategoryCommand
        {
            RandomizationId = randomizationId,
            CategoryId = categoryId,
            CategoryName = categoryName
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _usedQuestionRepositoryMock
            .Setup(x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(Unit.Value);

        _usedQuestionRepositoryMock.Verify(
            x => x.UpdateCategoryAsync(randomizationId, userId, categoryId, categoryName, cancellationToken),
            Times.Once);
    }
}
