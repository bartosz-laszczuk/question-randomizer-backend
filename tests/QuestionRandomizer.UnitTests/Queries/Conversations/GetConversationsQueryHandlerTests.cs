namespace QuestionRandomizer.UnitTests.Queries.Conversations;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Application.Queries.Conversations.GetConversations;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Unit tests for GetConversationsQueryHandler
/// </summary>
public class GetConversationsQueryHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetConversationsQueryHandler _handler;

    public GetConversationsQueryHandlerTests()
    {
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetConversationsQueryHandler(
            _conversationRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllUserConversations()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetConversationsQuery();

        var conversations = new List<Conversation>
        {
            new Conversation
            {
                Id = "conv1",
                Title = "First Conversation",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Conversation
            {
                Id = "conv2",
                Title = "Second Conversation",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("conv1");
        result[0].Title.Should().Be("First Conversation");
        result[1].Id.Should().Be("conv2");
        result[1].Title.Should().Be("Second Conversation");

        _currentUserServiceMock.Verify(x => x.GetUserId(), Times.Once);
        _conversationRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoConversationsFound_ReturnsEmptyList()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetConversationsQuery();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Conversation>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        // Arrange
        var userId = "test-user-123";
        var query = new GetConversationsQuery();
        var createdAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc);

        var conversations = new List<Conversation>
        {
            new Conversation
            {
                Id = "conv-detailed",
                Title = "Detailed Conversation",
                UserId = userId,
                IsActive = true,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            }
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _conversationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be("conv-detailed");
        dto.Title.Should().Be("Detailed Conversation");
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }
}
