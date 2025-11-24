namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Application.Commands.Questions.UpdateQuestion;
using QuestionRandomizer.Application.Commands.Questions.DeleteQuestion;
using QuestionRandomizer.Application.Queries.Questions.GetQuestions;
using QuestionRandomizer.Application.Queries.Questions.GetQuestionById;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Extension methods for mapping Question-related Minimal API endpoints
/// </summary>
public static class QuestionEndpoints
{
    /// <summary>
    /// Maps all question-related endpoints
    /// </summary>
    public static void MapQuestionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/questions")
            .RequireAuthorization()
            .WithTags("Questions");

        // GET /api/questions
        group.MapGet("", GetQuestions)
            .WithName("GetQuestions")
            .WithSummary("Get all questions for the authenticated user")
            .WithDescription("Retrieves all questions for the authenticated user with optional filtering by category and active status")
            .Produces<List<QuestionDto>>(StatusCodes.Status200OK);

        // GET /api/questions/{id}
        group.MapGet("{id}", GetQuestionById)
            .WithName("GetQuestionById")
            .WithSummary("Get a specific question by ID")
            .WithDescription("Retrieves a single question by its ID if the user is authorized")
            .Produces<QuestionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/questions
        group.MapPost("", CreateQuestion)
            .WithName("CreateQuestion")
            .WithSummary("Create a new question")
            .WithDescription("Creates a new question for the authenticated user")
            .Produces<QuestionDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // PUT /api/questions/{id}
        group.MapPut("{id}", UpdateQuestion)
            .WithName("UpdateQuestion")
            .WithSummary("Update an existing question")
            .WithDescription("Updates an existing question if the user is authorized")
            .Produces<QuestionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/questions/{id}
        group.MapDelete("{id}", DeleteQuestion)
            .WithName("DeleteQuestion")
            .WithSummary("Delete a question (soft delete)")
            .WithDescription("Soft deletes a question by setting IsActive to false")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Get all questions for the authenticated user
    /// </summary>
    private static async Task<Ok<List<QuestionDto>>> GetQuestions(
        IMediator mediator,
        string? categoryId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionsQuery
        {
            CategoryId = categoryId,
            IsActive = isActive
        };

        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Get a specific question by ID
    /// </summary>
    private static async Task<Ok<QuestionDto>> GetQuestionById(
        IMediator mediator,
        string id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionByIdQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Create a new question
    /// </summary>
    private static async Task<Created<QuestionDto>> CreateQuestion(
        IMediator mediator,
        CreateQuestionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/questions/{result.Id}", result);
    }

    /// <summary>
    /// Update an existing question
    /// </summary>
    private static async Task<Results<Ok<QuestionDto>, BadRequest<string>>> UpdateQuestion(
        IMediator mediator,
        string id,
        UpdateQuestionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest("ID in URL does not match ID in request body");
        }

        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Delete a question (soft delete)
    /// </summary>
    private static async Task<NoContent> DeleteQuestion(
        IMediator mediator,
        string id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteQuestionCommand { Id = id };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
