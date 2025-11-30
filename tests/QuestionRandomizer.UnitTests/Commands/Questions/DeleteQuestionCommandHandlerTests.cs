namespace QuestionRandomizer.UnitTests.Commands.Questions;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.Questions.DeleteQuestion;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for DeleteQuestionCommandHandler
/// </summary>
public class DeleteQuestionCommandHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteQuestionCommandHandler _handler;

    public DeleteQuestionCommandHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new DeleteQuestionCommandHandler(
            _questionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingQuestion_DeletesSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-456";
        var command = new DeleteQuestionCommand { Id = questionId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.DeleteAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.DeleteAsync(questionId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentQuestion_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "non-existent-question";
        var command = new DeleteQuestionCommand { Id = questionId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.DeleteAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Question with ID '{questionId}' was not found");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.DeleteAsync(questionId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentUser_UsesCurrentUserService()
    {
        // Arrange
        var userId1 = "user-1";
        var userId2 = "user-2";
        var questionId = "question-789";
        var command = new DeleteQuestionCommand { Id = questionId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId1);
        _questionRepositoryMock
            .Setup(x => x.DeleteAsync(questionId, userId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.DeleteAsync(questionId, userId1, It.IsAny<CancellationToken>()),
            Times.Once);
        _questionRepositoryMock.Verify(
            x => x.DeleteAsync(questionId, userId2, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_QuestionBelongingToAnotherUser_ReturnsFalseAndThrowsException()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-owned-by-another-user";
        var command = new DeleteQuestionCommand { Id = questionId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.DeleteAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _questionRepositoryMock.Verify(
            x => x.DeleteAsync(questionId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUnitValue()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-999";
        var command = new DeleteQuestionCommand { Id = questionId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.DeleteAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Unit>();
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_CancellationToken_IsPassedToRepository()
    {
        // Arrange
        var userId = "test-user-123";
        var questionId = "question-111";
        var command = new DeleteQuestionCommand { Id = questionId };
        var cancellationToken = new CancellationToken();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _questionRepositoryMock
            .Setup(x => x.DeleteAsync(questionId, userId, cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _questionRepositoryMock.Verify(
            x => x.DeleteAsync(questionId, userId, cancellationToken),
            Times.Once);
    }
}
