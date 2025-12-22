namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.UpdateQuestion;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.DeleteQuestion;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.CreateQuestionsBatch;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.UpdateQuestionsBatch;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.RemoveCategoryFromQuestions;
using QuestionRandomizer.Modules.Questions.Application.Commands.Questions.RemoveQualificationFromQuestions;
using QuestionRandomizer.Modules.Questions.Application.Queries.Questions.GetQuestions;
using QuestionRandomizer.Modules.Questions.Application.Queries.Questions.GetQuestionById;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

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
            .RequireAuthorization(AuthorizationPolicies.UserPolicy)
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

        // POST /api/questions/batch
        group.MapPost("batch", CreateQuestionsBatch)
            .WithName("CreateQuestionsBatch")
            .WithSummary("Create multiple questions in a batch")
            .WithDescription("Creates multiple questions for the authenticated user in a single transaction")
            .Produces<List<QuestionDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // PUT /api/questions/batch
        group.MapPut("batch", UpdateQuestionsBatch)
            .WithName("UpdateQuestionsBatch")
            .WithSummary("Update multiple questions in a batch")
            .WithDescription("Updates multiple questions in a single transaction")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        // DELETE /api/questions/category/{categoryId}
        group.MapDelete("category/{categoryId}", RemoveCategoryFromQuestions)
            .WithName("RemoveCategoryFromQuestions")
            .WithSummary("Remove category from all questions that reference it")
            .WithDescription("Removes the category ID and name from all questions that reference the specified category")
            .Produces(StatusCodes.Status204NoContent);

        // DELETE /api/questions/qualification/{qualificationId}
        group.MapDelete("qualification/{qualificationId}", RemoveQualificationFromQuestions)
            .WithName("RemoveQualificationFromQuestions")
            .WithSummary("Remove qualification from all questions that reference it")
            .WithDescription("Removes the qualification ID and name from all questions that reference the specified qualification")
            .Produces(StatusCodes.Status204NoContent);
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

    /// <summary>
    /// Create multiple questions in a batch
    /// </summary>
    private static async Task<Created<List<QuestionDto>>> CreateQuestionsBatch(
        IMediator mediator,
        CreateQuestionsBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created("/api/questions", result);
    }

    /// <summary>
    /// Update multiple questions in a batch
    /// </summary>
    private static async Task<NoContent> UpdateQuestionsBatch(
        IMediator mediator,
        UpdateQuestionsBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Remove category from all questions that reference it
    /// </summary>
    private static async Task<NoContent> RemoveCategoryFromQuestions(
        IMediator mediator,
        string categoryId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveCategoryFromQuestionsCommand { CategoryId = categoryId };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Remove qualification from all questions that reference it
    /// </summary>
    private static async Task<NoContent> RemoveQualificationFromQuestions(
        IMediator mediator,
        string qualificationId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveQualificationFromQuestionsCommand { QualificationId = qualificationId };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
