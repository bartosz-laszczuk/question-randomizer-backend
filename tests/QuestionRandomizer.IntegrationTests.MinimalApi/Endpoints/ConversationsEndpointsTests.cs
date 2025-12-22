namespace QuestionRandomizer.IntegrationTests.MinimalApi.Endpoints;
using QuestionRandomizer.Modules.Conversations.Domain.Entities;
using QuestionRandomizer.Modules.Randomization.Domain.Entities;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using QuestionRandomizer.Modules.Conversations.Application.Commands.Conversations.CreateConversation;
using QuestionRandomizer.Modules.Conversations.Application.Commands.Messages.AddMessage;
using QuestionRandomizer.Modules.Conversations.Application.DTOs;
using QuestionRandomizer.Modules.Questions.Domain.Entities;
using QuestionRandomizer.IntegrationTests.MinimalApi.Infrastructure;

/// <summary>
/// Integration tests for Conversations endpoints (Minimal API)
/// </summary>
public class ConversationsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ConversationsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.ResetMocks();
    }

    [Fact]
    public async Task GetConversations_ReturnsOkWithConversations()
    {
        // Arrange
        var conversations = new List<Conversation>
        {
            new()
            {
                Id = "conv1",
                Title = "AI Chat Session 1",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "conv2",
                Title = "AI Chat Session 2",
                UserId = CustomWebApplicationFactory.TestUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _factory.ConversationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations);

        // Act
        var response = await _client.GetAsync("/api/conversations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ConversationDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Id.Should().Be("conv1");
        result[1].Id.Should().Be("conv2");
    }

    [Fact]
    public async Task GetConversationById_ExistingConversation_ReturnsOk()
    {
        // Arrange
        var conversationId = "test-conversation-id";
        var conversation = new Conversation
        {
            Id = conversationId,
            Title = "Test Conversation",
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.ConversationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var response = await _client.GetAsync($"/api/conversations/{conversationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ConversationDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(conversationId);
        result.Title.Should().Be("Test Conversation");
    }

    [Fact]
    public async Task GetConversationById_NonExistentConversation_ReturnsNotFound()
    {
        // Arrange
        var conversationId = "non-existent-id";

        _factory.ConversationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act
        var response = await _client.GetAsync($"/api/conversations/{conversationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateConversation_ValidCommand_ReturnsCreated()
    {
        // Arrange
        var command = new CreateConversationCommand
        {
            Title = "New Conversation"
        };

        var createdConversation = new Conversation
        {
            Id = "new-conversation-id",
            Title = command.Title,
            UserId = CustomWebApplicationFactory.TestUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _factory.ConversationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdConversation);

        // Act
        var response = await _client.PostAsJsonAsync("/api/conversations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ConversationDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-conversation-id");
        result.Title.Should().Be(command.Title);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/conversations/{result.Id}");
    }

    [Fact]
    public async Task UpdateConversationTimestamp_ExistingConversation_ReturnsNoContent()
    {
        // Arrange
        var conversationId = "conversation-to-update";

        _factory.ConversationRepositoryMock
            .Setup(x => x.UpdateTimestampAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.PostAsync($"/api/conversations/{conversationId}/update-timestamp", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _factory.ConversationRepositoryMock.Verify(
            x => x.UpdateTimestampAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateConversationTimestamp_NonExistentConversation_ReturnsNotFound()
    {
        // Arrange
        var conversationId = "non-existent-id";

        _factory.ConversationRepositoryMock
            .Setup(x => x.UpdateTimestampAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _client.PostAsync($"/api/conversations/{conversationId}/update-timestamp", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteConversation_ExistingConversation_ReturnsNoContent()
    {
        // Arrange
        var conversationId = "conversation-to-delete";

        _factory.ConversationRepositoryMock
            .Setup(x => x.DeleteAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync($"/api/conversations/{conversationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _factory.ConversationRepositoryMock.Verify(
            x => x.DeleteAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteConversation_NonExistentConversation_ReturnsNotFound()
    {
        // Arrange
        var conversationId = "non-existent-id";

        _factory.ConversationRepositoryMock
            .Setup(x => x.DeleteAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _client.DeleteAsync($"/api/conversations/{conversationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMessages_ExistingConversation_ReturnsOkWithMessages()
    {
        // Arrange
        var conversationId = "test-conversation-id";
        var messages = new List<Message>
        {
            new()
            {
                Id = "msg1",
                ConversationId = conversationId,
                Role = "user",
                Content = "Hello AI",
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                Id = "msg2",
                ConversationId = conversationId,
                Role = "assistant",
                Content = "Hello! How can I help you?",
                Timestamp = DateTime.UtcNow
            }
        };

        _factory.ConversationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Conversation { Id = conversationId, UserId = CustomWebApplicationFactory.TestUserId });

        _factory.MessageRepositoryMock
            .Setup(x => x.GetByConversationIdAsync(
                conversationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        // Act
        var response = await _client.GetAsync($"/api/conversations/{conversationId}/messages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<MessageDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Id.Should().Be("msg1");
        result[0].Role.Should().Be("user");
        result[1].Id.Should().Be("msg2");
        result[1].Role.Should().Be("assistant");
    }

    [Fact]
    public async Task AddMessage_ValidCommand_ReturnsCreated()
    {
        // Arrange
        var conversationId = "test-conversation-id";
        var command = new AddMessageCommand
        {
            ConversationId = conversationId,
            Role = "user",
            Content = "Test message"
        };

        var createdMessage = new Message
        {
            Id = "new-message-id",
            ConversationId = conversationId,
            Role = command.Role,
            Content = command.Content,
            Timestamp = DateTime.UtcNow
        };

        _factory.ConversationRepositoryMock
            .Setup(x => x.GetByIdAsync(
                conversationId,
                CustomWebApplicationFactory.TestUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Conversation { Id = conversationId, UserId = CustomWebApplicationFactory.TestUserId });

        _factory.MessageRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<MessageDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-message-id");
        result.Role.Should().Be("user");
        result.Content.Should().Be("Test message");

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/conversations/{conversationId}/messages");
    }

    [Fact]
    public async Task AddMessage_ConversationIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var conversationId = "conversation-id-1";
        var command = new AddMessageCommand
        {
            ConversationId = "conversation-id-2", // Different from URL
            Role = "user",
            Content = "Test message"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/conversations/{conversationId}/messages", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
