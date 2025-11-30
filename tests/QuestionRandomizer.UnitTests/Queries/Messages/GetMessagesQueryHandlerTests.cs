namespace QuestionRandomizer.UnitTests.Queries.Messages;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Messages.GetMessages;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetMessagesQueryHandler
/// </summary>
public class GetMessagesQueryHandlerTests
{
    private readonly Mock<IMessageRepository> _messageRepositoryMock;
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetMessagesQueryHandler _handler;

    public GetMessagesQueryHandlerTests()
    {
        _messageRepositoryMock = new Mock<IMessageRepository>();
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetMessagesQueryHandler(
            _messageRepositoryMock.Object,
            _conversationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingConversationWithMessages_ReturnsMessagesList()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-456";
        var query = new GetMessagesQuery { ConversationId = conversationId };

        var conversation = new Conversation
        {
            Id = conversationId,
            Title = "Test Conversation",
            UserId = userId,
            IsActive = true
        };

        var messages = new List<Message>
        {
            new Message
            {
                Id = "msg-1",
                ConversationId = conversationId,
                Role = "user",
                Content = "Hello",
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            },
            new Message
            {
                Id = "msg-2",
                ConversationId = conversationId,
                Role = "assistant",
                Content = "Hi there!",
                Timestamp = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _messageRepositoryMock
            .Setup(x => x.GetByConversationIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("msg-1");
        result[0].Role.Should().Be("user");
        result[0].Content.Should().Be("Hello");
        result[1].Id.Should().Be("msg-2");
        result[1].Role.Should().Be("assistant");
        result[1].Content.Should().Be("Hi there!");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _conversationRepositoryMock.Verify(
            x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _messageRepositoryMock.Verify(
            x => x.GetByConversationIdAsync(conversationId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "non-existent-conv";
        var query = new GetMessagesQuery { ConversationId = conversationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation with ID {conversationId} not found");

        _messageRepositoryMock.Verify(
            x => x.GetByConversationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConversationWithNoMessages_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-empty";
        var query = new GetMessagesQuery { ConversationId = conversationId };

        var conversation = new Conversation
        {
            Id = conversationId,
            Title = "Empty Conversation",
            UserId = userId,
            IsActive = true
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _messageRepositoryMock
            .Setup(x => x.GetByConversationIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsAllMessageFieldsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-detailed";
        var query = new GetMessagesQuery { ConversationId = conversationId };
        var timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var conversation = new Conversation
        {
            Id = conversationId,
            Title = "Test",
            UserId = userId,
            IsActive = true
        };

        var messages = new List<Message>
        {
            new Message
            {
                Id = "msg-detailed",
                ConversationId = conversationId,
                Role = "system",
                Content = "System message content",
                Timestamp = timestamp
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _messageRepositoryMock
            .Setup(x => x.GetByConversationIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be("msg-detailed");
        dto.ConversationId.Should().Be(conversationId);
        dto.Role.Should().Be("system");
        dto.Content.Should().Be("System message content");
        dto.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public async Task Handle_UserDoesNotOwnConversation_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-other-user";
        var query = new GetMessagesQuery { ConversationId = conversationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
