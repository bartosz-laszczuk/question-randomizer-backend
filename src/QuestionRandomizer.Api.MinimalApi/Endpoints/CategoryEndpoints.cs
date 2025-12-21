namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.Infrastructure.Authorization;
using QuestionRandomizer.Application.Commands.Categories.CreateCategory;
using QuestionRandomizer.Application.Commands.Categories.CreateCategoriesBatch;
using QuestionRandomizer.Application.Commands.Categories.UpdateCategory;
using QuestionRandomizer.Application.Commands.Categories.DeleteCategory;
using QuestionRandomizer.Application.Queries.Categories.GetCategories;
using QuestionRandomizer.Application.Queries.Categories.GetCategoryById;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Minimal API endpoints for Categories
/// </summary>
public static class CategoryEndpoints
{
    /// <summary>
    /// Map all category endpoints
    /// </summary>
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .RequireAuthorization(AuthorizationPolicies.UserPolicy)
            .WithTags("Categories");

        group.MapGet("", GetCategories)
            .WithName("GetCategories")
            .Produces<List<CategoryDto>>(StatusCodes.Status200OK);

        group.MapGet("{id}", GetCategoryById)
            .WithName("GetCategoryById")
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateCategory)
            .WithName("CreateCategory")
            .Produces<CategoryDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("batch", CreateCategoriesBatch)
            .WithName("CreateCategoriesBatch")
            .Produces<List<CategoryDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("{id}", UpdateCategory)
            .WithName("UpdateCategory")
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("{id}", DeleteCategory)
            .WithName("DeleteCategory")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<List<CategoryDto>>> GetCategories(
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CategoryDto>, NotFound>> GetCategoryById(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryByIdQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Created<CategoryDto>> CreateCategory(
        [FromBody] CreateCategoryCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/categories/{result.Id}", result);
    }

    private static async Task<Created<List<CategoryDto>>> CreateCategoriesBatch(
        [FromBody] CreateCategoriesBatchCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created("/api/categories", result);
    }

    private static async Task<Results<Ok<CategoryDto>, BadRequest<string>, NotFound>> UpdateCategory(
        string id,
        [FromBody] UpdateCategoryCommand command,
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

    private static async Task<Results<NoContent, NotFound>> DeleteCategory(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryCommand { Id = id };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
