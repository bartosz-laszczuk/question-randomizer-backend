namespace QuestionRandomizer.E2ETests.Workflows;

using System.Net;
using Bogus;
using FluentAssertions;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.E2ETests.Infrastructure;

/// <summary>
/// End-to-End tests for the complete Conversation workflow
/// Tests conversation creation, message management, and conversation lifecycle
/// </summary>
public class ConversationWorkflowE2ETests : E2ETestBase
{
    private readonly Faker _faker;

    public ConversationWorkflowE2ETests(E2ETestWebApplicationFactory factory) : base(factory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task CompleteConversationWorkflow_ShouldSucceed()
    {
        // Step 1: Create a conversation with a title
        var title = CreateTestString("E2E Conversation");
        var createConversationRequest = new { Title = title };

        var conversation = await PostAsync<object, ConversationDto>(
            "/api/conversations",
            createConversationRequest);

        conversation.Should().NotBeNull();
        conversation!.Id.Should().NotBeEmpty();
        conversation.Title.Should().Be(title);
        conversation.IsActive.Should().BeTrue();
        conversation.CreatedAt.Should().NotBeNull();

        // Step 2: Add user message to conversation
        var userMessageContent = "What is the capital of France?";
        var addUserMessageRequest = new
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = userMessageContent
        };

        var userMessage = await PostAsync<object, MessageDto>(
            $"/api/conversations/{conversation.Id}/messages",
            addUserMessageRequest);

        userMessage.Should().NotBeNull();
        userMessage!.Id.Should().NotBeEmpty();
        userMessage.ConversationId.Should().Be(conversation.Id);
        userMessage.Role.Should().Be("user");
        userMessage.Content.Should().Be(userMessageContent);
        userMessage.Timestamp.Should().NotBeNull();

        // Step 3: Add assistant message to conversation
        var assistantMessageContent = "The capital of France is Paris.";
        var addAssistantMessageRequest = new
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = assistantMessageContent
        };

        var assistantMessage = await PostAsync<object, MessageDto>(
            $"/api/conversations/{conversation.Id}/messages",
            addAssistantMessageRequest);

        assistantMessage.Should().NotBeNull();
        assistantMessage!.Id.Should().NotBeEmpty();
        assistantMessage.ConversationId.Should().Be(conversation.Id);
        assistantMessage.Role.Should().Be("assistant");
        assistantMessage.Content.Should().Be(assistantMessageContent);
        assistantMessage.Timestamp.Should().BeAfter(userMessage.Timestamp!.Value);

        // Step 4: Get conversation by ID - verify it exists
        var retrievedConversation = await GetAsync<ConversationDto>($"/api/conversations/{conversation.Id}");

        retrievedConversation.Should().NotBeNull();
        retrievedConversation!.Id.Should().Be(conversation.Id);
        retrievedConversation.Title.Should().Be(title);

        // Step 5: Get all conversations - verify our conversation is there
        var allConversations = await GetAsync<List<ConversationDto>>("/api/conversations");

        allConversations.Should().NotBeNull();
        allConversations.Should().Contain(c => c.Id == conversation.Id);

        var conversationInList = allConversations!.First(c => c.Id == conversation.Id);
        conversationInList.Title.Should().Be(title);

        // Step 6: Get messages for the conversation - verify both messages are there
        var messages = await GetAsync<List<MessageDto>>($"/api/conversations/{conversation.Id}/messages");

        messages.Should().NotBeNull();
        messages.Should().HaveCount(2);
        messages.Should().Contain(m => m.Id == userMessage.Id && m.Role == "user");
        messages.Should().Contain(m => m.Id == assistantMessage.Id && m.Role == "assistant");

        // Messages should be ordered by timestamp (oldest first)
        messages![0].Timestamp.Should().NotBeNull();
        messages[1].Timestamp.Should().NotBeNull();
        messages[0].Timestamp!.Value.Should().BeOnOrBefore(messages[1].Timestamp!.Value);

        // Step 7: Update conversation timestamp
        var updateTimestampResponse = await Client.PostAsync(
            $"/api/conversations/{conversation.Id}/update-timestamp",
            null);

        AssertSuccess(updateTimestampResponse);
        updateTimestampResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 8: Delete conversation
        var deleteResponse = await DeleteAsync($"/api/conversations/{conversation.Id}");

        AssertSuccess(deleteResponse);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 9: Verify 404 when getting deleted conversation
        var getDeletedResponse = await GetAsync($"/api/conversations/{conversation.Id}");

        AssertNotFound(getDeletedResponse);

        // Step 10: Verify deleted conversation is not in the list
        var conversationsAfterDelete = await GetAsync<List<ConversationDto>>("/api/conversations");

        conversationsAfterDelete.Should().NotBeNull();
        conversationsAfterDelete.Should().NotContain(c => c.Id == conversation.Id);
    }

    [Fact]
    public async Task ConversationWorkflow_MultipleMessages_ShouldMaintainOrder()
    {
        // Arrange
        var conversation = await CreateTestConversation("Multi-message Test");

        // Act - Add multiple messages in sequence
        var message1 = await AddMessage(conversation, "user", "Question 1");
        await WaitAsync(50); // Small delay to ensure different timestamps

        var message2 = await AddMessage(conversation, "assistant", "Answer 1");
        await WaitAsync(50);

        var message3 = await AddMessage(conversation, "user", "Question 2");
        await WaitAsync(50);

        var message4 = await AddMessage(conversation, "assistant", "Answer 2");

        // Assert - Get messages and verify order
        var messages = await GetAsync<List<MessageDto>>($"/api/conversations/{conversation}/messages");

        messages.Should().HaveCount(4);
        messages![0].Id.Should().Be(message1);
        messages[1].Id.Should().Be(message2);
        messages[2].Id.Should().Be(message3);
        messages[3].Id.Should().Be(message4);

        // Verify timestamps are in ascending order
        for (int i = 0; i < messages.Count - 1; i++)
        {
            messages[i].Timestamp.Should().NotBeNull();
            messages[i + 1].Timestamp.Should().NotBeNull();
            messages[i].Timestamp!.Value.Should().BeOnOrBefore(messages[i + 1].Timestamp!.Value);
        }
    }

    [Fact]
    public async Task ConversationWorkflow_MultipleConversations_ShouldTrackSeparately()
    {
        // Arrange - Create two conversations
        var conv1 = await CreateTestConversation("Conversation 1");
        var conv2 = await CreateTestConversation("Conversation 2");

        // Act - Add different messages to each conversation
        await AddMessage(conv1, "user", "Message in Conv 1");
        await AddMessage(conv2, "user", "Message in Conv 2");
        await AddMessage(conv1, "assistant", "Response in Conv 1");

        // Assert - Verify messages are tracked separately
        var messagesConv1 = await GetAsync<List<MessageDto>>($"/api/conversations/{conv1}/messages");
        var messagesConv2 = await GetAsync<List<MessageDto>>($"/api/conversations/{conv2}/messages");

        messagesConv1.Should().HaveCount(2);
        messagesConv2.Should().HaveCount(1);

        messagesConv1.Should().OnlyContain(m => m.ConversationId == conv1);
        messagesConv2.Should().OnlyContain(m => m.ConversationId == conv2);
    }

    [Fact]
    public async Task CreateConversation_WithEmptyTitle_ShouldSucceed()
    {
        // Arrange
        var createRequest = new { Title = "" };

        // Act
        var conversation = await PostAsync<object, ConversationDto>("/api/conversations", createRequest);

        // Assert
        conversation.Should().NotBeNull();
        conversation!.Title.Should().BeEmpty();
        conversation.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetConversation_WithInvalidId_ShouldReturn404()
    {
        // Act
        var response = await GetAsync("/api/conversations/non-existent-id");

        // Assert
        AssertNotFound(response);
    }

    [Fact]
    public async Task DeleteConversation_WithInvalidId_ShouldReturn404()
    {
        // Act
        var response = await DeleteAsync("/api/conversations/non-existent-id");

        // Assert
        AssertNotFound(response);
    }

    [Fact]
    public async Task AddMessage_WithInvalidConversationId_ShouldReturn404()
    {
        // Arrange
        var addMessageRequest = new
        {
            ConversationId = "non-existent-id",
            Role = "user",
            Content = "Test message"
        };

        // Act
        var response = await PostAsync("/api/conversations/non-existent-id/messages", addMessageRequest);

        // Assert
        AssertNotFound(response);
    }

    [Fact]
    public async Task AddMessage_WithMismatchedConversationId_ShouldReturnBadRequest()
    {
        // Arrange
        var conversation = await CreateTestConversation("Test Conversation");

        var addMessageRequest = new
        {
            ConversationId = "different-id", // Doesn't match URL
            Role = "user",
            Content = "Test message"
        };

        // Act
        var response = await PostAsync($"/api/conversations/{conversation}/messages", addMessageRequest);

        // Assert
        AssertBadRequest(response);
    }

    [Fact]
    public async Task GetMessages_ForEmptyConversation_ShouldReturnEmptyList()
    {
        // Arrange
        var conversation = await CreateTestConversation("Empty Conversation");

        // Act
        var messages = await GetAsync<List<MessageDto>>($"/api/conversations/{conversation}/messages");

        // Assert
        messages.Should().NotBeNull();
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateConversationTimestamp_WithInvalidId_ShouldReturn404()
    {
        // Act
        var response = await Client.PostAsync(
            "/api/conversations/non-existent-id/update-timestamp",
            null);

        // Assert
        AssertNotFound(response);
    }

    /// <summary>
    /// Helper method to create a test conversation
    /// </summary>
    private async Task<string> CreateTestConversation(string titlePrefix)
    {
        var title = CreateTestString(titlePrefix);
        var createRequest = new { Title = title };

        var conversation = await PostAsync<object, ConversationDto>("/api/conversations", createRequest);
        return conversation!.Id;
    }

    /// <summary>
    /// Helper method to add a message to a conversation
    /// </summary>
    private async Task<string> AddMessage(string conversationId, string role, string content)
    {
        var addMessageRequest = new
        {
            ConversationId = conversationId,
            Role = role,
            Content = content
        };

        var message = await PostAsync<object, MessageDto>(
            $"/api/conversations/{conversationId}/messages",
            addMessageRequest);

        return message!.Id;
    }
}
