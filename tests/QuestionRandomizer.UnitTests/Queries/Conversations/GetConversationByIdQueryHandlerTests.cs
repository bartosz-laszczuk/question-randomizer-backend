namespace QuestionRandomizer.UnitTests.Queries.Conversations;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Conversations.GetConversationById;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetConversationByIdQueryHandler
/// </summary>
public class GetConversationByIdQueryHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetConversationByIdQueryHandler _handler;

    public GetConversationByIdQueryHandlerTests()
    {
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetConversationByIdQueryHandler(
            _conversationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingConversation_ReturnsConversationDto()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-456";
        var query = new GetConversationByIdQuery { Id = conversationId };

        var conversation = new Conversation
        {
            Id = conversationId,
            Title = "Test Conversation",
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(conversationId);
        result.Title.Should().Be("Test Conversation");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().NotBeNull();

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _conversationRepositoryMock.Verify(
            x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentConversation_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "non-existent-conv";
        var query = new GetConversationByIdQuery { Id = conversationId };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conversation with ID {conversationId} not found");
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-detailed";
        var query = new GetConversationByIdQuery { Id = conversationId };
        var createdAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc);

        var conversation = new Conversation
        {
            Id = conversationId,
            Title = "Detailed Conversation",
            UserId = userId,
            IsActive = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByIdAsync(conversationId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.Should().Be(conversationId);
        result.Title.Should().Be("Detailed Conversation");
        result.IsActive.Should().BeFalse();
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task Handle_UserDoesNotOwnConversation_ThrowsNotFoundException()
    {
        // Arrange
        var userId = "test-user-123";
        var conversationId = "conv-other-user";
        var query = new GetConversationByIdQuery { Id = conversationId };

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
