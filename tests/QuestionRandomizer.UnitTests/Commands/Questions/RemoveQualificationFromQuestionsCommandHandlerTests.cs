namespace QuestionRandomizer.UnitTests.Commands.Questions;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.Questions.RemoveQualificationFromQuestions;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for RemoveQualificationFromQuestionsCommandHandler
/// </summary>
public class RemoveQualificationFromQuestionsCommandHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly RemoveQualificationFromQuestionsCommandHandler _handler;

    public RemoveQualificationFromQuestionsCommandHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new RemoveQualificationFromQuestionsCommandHandler(
            _questionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesQualificationFromQuestions()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-456";

        var command = new RemoveQualificationFromQuestionsCommand
        {
            QualificationId = qualificationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.RemoveQualificationIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _questionRepositoryMock.Verify(
            x => x.RemoveQualificationIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentQualificationId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "different-qual-789";

        var command = new RemoveQualificationFromQuestionsCommand
        {
            QualificationId = qualificationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.RemoveQualificationIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.RemoveQualificationIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        var userId = "test-user-123";
        var qualificationId = "qual-456";
        var cancellationToken = new CancellationToken();

        var command = new RemoveQualificationFromQuestionsCommand
        {
            QualificationId = qualificationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.RemoveQualificationIdAsync(qualificationId, userId, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.RemoveQualificationIdAsync(qualificationId, userId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesCurrentUserId()
    {
        // Arrange
        var userId = "specific-user-999";
        var qualificationId = "qual-456";

        var command = new RemoveQualificationFromQuestionsCommand
        {
            QualificationId = qualificationId
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock
            .Setup(x => x.RemoveQualificationIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _questionRepositoryMock.Verify(
            x => x.RemoveQualificationIdAsync(qualificationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
