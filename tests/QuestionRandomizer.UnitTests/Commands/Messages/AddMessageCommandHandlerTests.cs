namespace QuestionRandomizer.UnitTests.Commands.Messages;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Messages.AddMessage;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for AddMessageCommandHandler
/// </summary>
public class AddMessageCommandHandlerTests
{
    private readonly Mock<IMessageRepository> _messageRepositoryMock;
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AddMessageCommandHandler _handler;

    public AddMessageCommandHandlerTests()
    {
        _messageRepositoryMock = new Mock<IMessageRepository>();
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new AddMessageCommandHandler(
            _messageRepositoryMock.Object,
            _conversationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsMessageSuccessfully()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-456";
        var command = new AddMessageCommand
        {
            ConversationId = conversationId,
            Role = "user",
            Content = "Hello, how can I help?"
        };

        var conversation = new Conversation
        {
            Id = conversationId,
            Title = "Test Conversation",
            UserId = userId,
            IsActive = true
        };

        var createdMessage = new Message
        {
            Id = "msg-789",
            ConversationId = conversationId,
            Role = command.Role,
            Content = command.Content,
            Timestamp = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _messageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("msg-789");
        result.ConversationId.Should().Be(conversationId);
        result.Role.Should().Be("user");
        result.Content.Should().Be("Hello, how can I help?");
        result.Timestamp.Should().NotBeNull();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _conversationRepositoryMock.Verify(
            x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _messageRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Message>(m =>
                    m.ConversationId == conversationId &&
                    m.Role == "user" &&
                    m.Content == "Hello, how can I help?"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "non-existent-conv";
        var command = new AddMessageCommand
        {
            ConversationId = conversationId,
            Role = "user",
            Content = "Test message"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation with ID {conversationId} not found");

        _messageRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SetsTimestampToUtcNow()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-999";
        var command = new AddMessageCommand
        {
            ConversationId = conversationId,
            Role = "assistant",
            Content = "Response message"
        };

        var beforeCreate = DateTime.UtcNow;

        var conversation = new Conversation
        {
            Id = conversationId,
            Title = "Test",
            UserId = userId,
            IsActive = true
        };

        var createdMessage = new Message
        {
            Id = "msg-111",
            ConversationId = conversationId,
            Role = command.Role,
            Content = command.Content,
            Timestamp = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _messageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.Timestamp.Should().NotBeNull();
        result.Timestamp.Should().BeOnOrAfter(beforeCreate);
        result.Timestamp.Should().BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task Handle_UserDoesNotOwnConversation_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-other-user";
        var command = new AddMessageCommand
        {
            ConversationId = conversationId,
            Role = "user",
            Content = "Test message"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
