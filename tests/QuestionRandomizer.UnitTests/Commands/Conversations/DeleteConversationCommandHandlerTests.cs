namespace QuestionRandomizer.UnitTests.Commands.Conversations;

using FluentAssertions;
using MediatR;
using Moq;
using QuestionRandomizer.Application.Commands.Conversations.DeleteConversation;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for DeleteConversationCommandHandler
/// </summary>
public class DeleteConversationCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteConversationCommandHandler _handler;

    public DeleteConversationCommandHandlerTests()
    {
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new DeleteConversationCommandHandler(
            _conversationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingConversation_DeletesSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-456";
        var command = new DeleteConversationCommand { Id = conversationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.DeleteAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _conversationRepositoryMock.Verify(
            x => x.DeleteAsync(conversationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentConversation_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "non-existent-conv";
        var command = new DeleteConversationCommand { Id = conversationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.DeleteAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation with ID {conversationId} not found");

        _conversationRepositoryMock.Verify(
            x => x.DeleteAsync(conversationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsUnitValue()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-789";
        var command = new DeleteConversationCommand { Id = conversationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.DeleteAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Unit>();
        result.Should().Be(Unit.Value);
    }
}
