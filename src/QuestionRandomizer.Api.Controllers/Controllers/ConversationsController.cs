namespace QuestionRandomizer.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.Modules.Conversations.Application.Commands.Conversations.CreateConversation;
using QuestionRandomizer.Modules.Conversations.Application.Commands.Conversations.UpdateConversationTimestamp;
using QuestionRandomizer.Modules.Conversations.Application.Commands.Conversations.DeleteConversation;
using QuestionRandomizer.Modules.Conversations.Application.Commands.Messages.AddMessage;
using QuestionRandomizer.Modules.Conversations.Application.Queries.Conversations.GetConversations;
using QuestionRandomizer.Modules.Conversations.Application.Queries.Conversations.GetConversationById;
using QuestionRandomizer.Modules.Conversations.Application.Queries.Messages.GetMessages;
using QuestionRandomizer.Modules.Conversations.Application.DTOs;

/// <summary>
/// Manages AI chat conversations and messages
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.UserPolicy)]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all conversations for the authenticated user
    /// </summary>
    /// <returns>List of conversations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations(CancellationToken cancellationToken = default)
    {
        var query = new GetConversationsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific conversation by ID
    /// </summary>
    /// <param name="id">Conversation ID</param>
    /// <returns>Conversation details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversationById(string id, CancellationToken cancellationToken = default)
    {
        var query = new GetConversationByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new conversation
    /// </summary>
    /// <param name="command">Conversation details</param>
    /// <returns>Created conversation</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetConversationById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update the timestamp of a conversation
    /// </summary>
    /// <param name="id">Conversation ID</param>
    /// <returns>No content on success</returns>
    [HttpPost("{id}/update-timestamp")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConversationTimestamp(string id, CancellationToken cancellationToken = default)
    {
        var command = new UpdateConversationTimestampCommand { ConversationId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete a conversation
    /// </summary>
    /// <param name="id">Conversation ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConversation(string id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteConversationCommand { Id = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get all messages in a conversation
    /// </summary>
    /// <param name="id">Conversation ID</param>
    /// <returns>List of messages</returns>
    [HttpGet("{id}/messages")]
    [ProducesResponseType(typeof(List<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(string id, CancellationToken cancellationToken = default)
    {
        var query = new GetMessagesQuery { ConversationId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Add a message to a conversation
    /// </summary>
    /// <param name="id">Conversation ID</param>
    /// <param name="command">Message details</param>
    /// <returns>Created message</returns>
    [HttpPost("{id}/messages")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMessage(string id, [FromBody] AddMessageCommand command, CancellationToken cancellationToken = default)
    {
        if (id != command.ConversationId)
        {
            return BadRequest("Conversation ID in URL does not match ID in request body");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMessages), new { id = command.ConversationId }, result);
    }
}
