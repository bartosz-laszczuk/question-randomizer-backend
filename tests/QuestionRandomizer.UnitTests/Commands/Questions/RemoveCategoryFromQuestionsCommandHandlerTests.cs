namespace QuestionRandomizer.UnitTests.Commands.Questions;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.Questions.RemoveCategoryFromQuestions;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for RemoveCategoryFromQuestionsCommandHandler
/// </summary>
public class RemoveCategoryFromQuestionsCommandHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly RemoveCategoryFromQuestionsCommandHandler _handler;

    public RemoveCategoryFromQuestionsCommandHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new RemoveCategoryFromQuestionsCommandHandler(
            _questionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesCategoryFromQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";

        var command = new RemoveCategoryFromQuestionsCommand
        {
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.RemoveCategoryIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.RemoveCategoryIdAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentCategoryId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "different-category-789";

        var command = new RemoveCategoryFromQuestionsCommand
        {
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.RemoveCategoryIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.RemoveCategoryIdAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var categoryId = "category-456";
        var cancellationToken = new CancellationToken();

        var command = new RemoveCategoryFromQuestionsCommand
        {
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.RemoveCategoryIdAsync(categoryId, userId, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.RemoveCategoryIdAsync(categoryId, userId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var categoryId = "category-456";

        var command = new RemoveCategoryFromQuestionsCommand
        {
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.RemoveCategoryIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.RemoveCategoryIdAsync(categoryId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
