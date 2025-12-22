namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.Modules.Randomization.Application.Commands.Randomizations.CreateRandomization;
using QuestionRandomizer.Modules.Randomization.Application.Commands.Randomizations.UpdateRandomization;
using QuestionRandomizer.Modules.Randomization.Application.Commands.Randomizations.ClearCurrentQuestion;
using QuestionRandomizer.Modules.Randomization.Application.Commands.SelectedCategories.AddSelectedCategory;
using QuestionRandomizer.Modules.Randomization.Application.Commands.SelectedCategories.DeleteSelectedCategory;
using QuestionRandomizer.Modules.Randomization.Application.Commands.UsedQuestions.AddUsedQuestion;
using QuestionRandomizer.Modules.Randomization.Application.Commands.UsedQuestions.DeleteUsedQuestion;
using QuestionRandomizer.Modules.Randomization.Application.Commands.UsedQuestions.UpdateUsedQuestionCategory;
using QuestionRandomizer.Modules.Randomization.Application.Commands.PostponedQuestions.AddPostponedQuestion;
using QuestionRandomizer.Modules.Randomization.Application.Commands.PostponedQuestions.DeletePostponedQuestion;
using QuestionRandomizer.Modules.Randomization.Application.Commands.PostponedQuestions.UpdatePostponedQuestionTimestamp;
using QuestionRandomizer.Modules.Randomization.Application.Queries.Randomizations.GetRandomization;
using QuestionRandomizer.Modules.Randomization.Application.Queries.SelectedCategories.GetSelectedCategories;
using QuestionRandomizer.Modules.Randomization.Application.Queries.UsedQuestions.GetUsedQuestions;
using QuestionRandomizer.Modules.Randomization.Application.Queries.PostponedQuestions.GetPostponedQuestions;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;

/// <summary>
/// Minimal API endpoints for randomizations
/// </summary>
public static class RandomizationEndpoints
{
    public static void MapRandomizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/randomizations")
            .RequireAuthorization(AuthorizationPolicies.UserPolicy)
            .WithTags("Randomizations");

        group.MapGet("", GetRandomization)
            .WithName("GetRandomization")
            .WithSummary("Get the active randomization session for the authenticated user")
            .Produces<RandomizationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateRandomization)
            .WithName("CreateRandomization")
            .WithSummary("Create a new randomization session")
            .Produces<RandomizationDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("{id}", UpdateRandomization)
            .WithName("UpdateRandomization")
            .WithSummary("Update an existing randomization session")
            .Produces<RandomizationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("{id}/clear-current-question", ClearCurrentQuestion)
            .WithName("ClearCurrentQuestion")
            .WithSummary("Clear the current question from the randomization session")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // ================ Selected Categories ================

        group.MapGet("{id}/selected-categories", GetSelectedCategories)
            .WithName("GetSelectedCategories")
            .WithSummary("Get all selected categories for a randomization session")
            .Produces<List<SelectedCategoryDto>>(StatusCodes.Status200OK);

        group.MapPost("{id}/selected-categories", AddSelectedCategory)
            .WithName("AddSelectedCategory")
            .WithSummary("Add a category to the selected categories list")
            .Produces<SelectedCategoryDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("{id}/selected-categories/{categoryId}", DeleteSelectedCategory)
            .WithName("DeleteSelectedCategory")
            .WithSummary("Remove a category from the selected categories list")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // ================ Used Questions ================

        group.MapGet("{id}/used-questions", GetUsedQuestions)
            .WithName("GetUsedQuestions")
            .WithSummary("Get all used questions for a randomization session")
            .Produces<List<UsedQuestionDto>>(StatusCodes.Status200OK);

        group.MapPost("{id}/used-questions", AddUsedQuestion)
            .WithName("AddUsedQuestion")
            .WithSummary("Add a question to the used questions list")
            .Produces<UsedQuestionDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("{id}/used-questions/{questionId}", DeleteUsedQuestion)
            .WithName("DeleteUsedQuestion")
            .WithSummary("Remove a question from the used questions list")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("{id}/used-questions/category", UpdateUsedQuestionCategory)
            .WithName("UpdateUsedQuestionCategory")
            .WithSummary("Update category information for used questions")
            .Produces(StatusCodes.Status204NoContent);

        // ================ Postponed Questions ================

        group.MapGet("{id}/postponed-questions", GetPostponedQuestions)
            .WithName("GetPostponedQuestions")
            .WithSummary("Get all postponed questions for a randomization session")
            .Produces<List<PostponedQuestionDto>>(StatusCodes.Status200OK);

        group.MapPost("{id}/postponed-questions", AddPostponedQuestion)
            .WithName("AddPostponedQuestion")
            .WithSummary("Add a question to the postponed questions list")
            .Produces<PostponedQuestionDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("{id}/postponed-questions/{questionId}", DeletePostponedQuestion)
            .WithName("DeletePostponedQuestion")
            .WithSummary("Remove a question from the postponed questions list")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("{id}/postponed-questions/{questionId}/timestamp", UpdatePostponedQuestionTimestamp)
            .WithName("UpdatePostponedQuestionTimestamp")
            .WithSummary("Update the timestamp of a postponed question")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<RandomizationDto>, NotFound>> GetRandomization(
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRandomizationQuery();
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Created<RandomizationDto>> CreateRandomization(
        [FromBody] CreateRandomizationCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/randomizations/{result.Id}", result);
    }

    private static async Task<Results<Ok<RandomizationDto>, NotFound, BadRequest<string>>> UpdateRandomization(
        string id,
        [FromBody] UpdateRandomizationCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest("ID in URL does not match ID in request body");
        }

        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, NotFound>> ClearCurrentQuestion(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new ClearCurrentQuestionCommand { RandomizationId = id };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    // ================ Selected Categories Handlers ================

    private static async Task<Ok<List<SelectedCategoryDto>>> GetSelectedCategories(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSelectedCategoriesQuery { RandomizationId = id };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Created<SelectedCategoryDto>, BadRequest<string>>> AddSelectedCategory(
        string id,
        [FromBody] AddSelectedCategoryCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        if (id != command.RandomizationId)
        {
            return TypedResults.BadRequest("Randomization ID in URL does not match ID in request body");
        }

        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/randomizations/{id}/selected-categories", result);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteSelectedCategory(
        string id,
        string categoryId,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteSelectedCategoryCommand
        {
            RandomizationId = id,
            CategoryId = categoryId
        };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    // ================ Used Questions Handlers ================

    private static async Task<Ok<List<UsedQuestionDto>>> GetUsedQuestions(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsedQuestionsQuery { RandomizationId = id };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Created<UsedQuestionDto>, BadRequest<string>>> AddUsedQuestion(
        string id,
        [FromBody] AddUsedQuestionCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        if (id != command.RandomizationId)
        {
            return TypedResults.BadRequest("Randomization ID in URL does not match ID in request body");
        }

        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/randomizations/{id}/used-questions", result);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteUsedQuestion(
        string id,
        string questionId,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteUsedQuestionCommand
        {
            RandomizationId = id,
            QuestionId = questionId
        };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, BadRequest<string>>> UpdateUsedQuestionCategory(
        string id,
        [FromBody] UpdateUsedQuestionCategoryCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        if (id != command.RandomizationId)
        {
            return TypedResults.BadRequest("Randomization ID in URL does not match ID in request body");
        }

        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    // ================ Postponed Questions Handlers ================

    private static async Task<Ok<List<PostponedQuestionDto>>> GetPostponedQuestions(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPostponedQuestionsQuery { RandomizationId = id };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Created<PostponedQuestionDto>, BadRequest<string>>> AddPostponedQuestion(
        string id,
        [FromBody] AddPostponedQuestionCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        if (id != command.RandomizationId)
        {
            return TypedResults.BadRequest("Randomization ID in URL does not match ID in request body");
        }

        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/randomizations/{id}/postponed-questions", result);
    }

    private static async Task<Results<NoContent, NotFound>> DeletePostponedQuestion(
        string id,
        string questionId,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new DeletePostponedQuestionCommand
        {
            RandomizationId = id,
            QuestionId = questionId
        };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> UpdatePostponedQuestionTimestamp(
        string id,
        string questionId,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePostponedQuestionTimestampCommand
        {
            RandomizationId = id,
            QuestionId = questionId
        };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
