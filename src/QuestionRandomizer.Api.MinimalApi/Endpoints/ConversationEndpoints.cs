namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
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
/// Minimal API endpoints for conversations and messages
/// </summary>
public static class ConversationEndpoints
{
    public static void MapConversationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conversations")
            .RequireAuthorization(AuthorizationPolicies.UserPolicy)
            .WithTags("Conversations");

        // Conversation endpoints
        group.MapGet("", GetConversations)
            .WithName("GetConversations")
            .WithSummary("Get all conversations for the authenticated user")
            .Produces<List<ConversationDto>>(StatusCodes.Status200OK);

        group.MapGet("{id}", GetConversationById)
            .WithName("GetConversationById")
            .WithSummary("Get a specific conversation by ID")
            .Produces<ConversationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateConversation)
            .WithName("CreateConversation")
            .WithSummary("Create a new conversation")
            .Produces<ConversationDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("{id}/update-timestamp", UpdateConversationTimestamp)
            .WithName("UpdateConversationTimestamp")
            .WithSummary("Update the timestamp of a conversation")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("{id}", DeleteConversation)
            .WithName("DeleteConversation")
            .WithSummary("Delete a conversation")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // Message endpoints (nested under conversations)
        group.MapGet("{id}/messages", GetMessages)
            .WithName("GetMessages")
            .WithSummary("Get all messages in a conversation")
            .Produces<List<MessageDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("{id}/messages", AddMessage)
            .WithName("AddMessage")
            .WithSummary("Add a message to a conversation")
            .Produces<MessageDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<List<ConversationDto>>> GetConversations(
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetConversationsQuery();
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ConversationDto>, NotFound>> GetConversationById(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetConversationByIdQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Created<ConversationDto>> CreateConversation(
        [FromBody] CreateConversationCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/conversations/{result.Id}", result);
    }

    private static async Task<Results<NoContent, NotFound>> UpdateConversationTimestamp(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateConversationTimestampCommand { ConversationId = id };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteConversation(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteConversationCommand { Id = id };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<List<MessageDto>>, NotFound>> GetMessages(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMessagesQuery { ConversationId = id };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Created<MessageDto>, BadRequest<string>, NotFound>> AddMessage(
        string id,
        [FromBody] AddMessageCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        if (id != command.ConversationId)
        {
            return TypedResults.BadRequest("Conversation ID in URL does not match ID in request body");
        }

        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/conversations/{command.ConversationId}/messages", result);
    }
}
