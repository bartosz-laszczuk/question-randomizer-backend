namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualification;
using QuestionRandomizer.Application.Commands.Qualifications.UpdateQualification;
using QuestionRandomizer.Application.Commands.Qualifications.DeleteQualification;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualificationsBatch;
using QuestionRandomizer.Application.Queries.Qualifications.GetQualifications;
using QuestionRandomizer.Application.Queries.Qualifications.GetQualificationById;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Minimal API endpoints for qualifications
/// </summary>
public static class QualificationEndpoints
{
    public static void MapQualificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/qualifications")
            .WithTags("Qualifications");

        group.MapGet("", GetQualifications)
            .WithName("GetQualifications")
            .WithSummary("Get all qualifications for the authenticated user")
            .Produces<List<QualificationDto>>(StatusCodes.Status200OK);

        group.MapGet("{id}", GetQualificationById)
            .WithName("GetQualificationById")
            .WithSummary("Get a specific qualification by ID")
            .Produces<QualificationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateQualification)
            .WithName("CreateQualification")
            .WithSummary("Create a new qualification")
            .Produces<QualificationDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("batch", CreateQualificationsBatch)
            .WithName("CreateQualificationsBatch")
            .WithSummary("Create multiple qualifications from a list of names")
            .Produces<List<QualificationDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("{id}", UpdateQualification)
            .WithName("UpdateQualification")
            .WithSummary("Update an existing qualification")
            .Produces<QualificationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("{id}", DeleteQualification)
            .WithName("DeleteQualification")
            .WithSummary("Delete a qualification (soft delete)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<List<QualificationDto>>> GetQualifications(
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQualificationsQuery();
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<QualificationDto>, NotFound>> GetQualificationById(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQualificationByIdQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Created<QualificationDto>> CreateQualification(
        [FromBody] CreateQualificationCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/qualifications/{result.Id}", result);
    }

    private static async Task<Created<List<QualificationDto>>> CreateQualificationsBatch(
        [FromBody] CreateQualificationsBatchCommand command,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created("/api/qualifications", result);
    }

    private static async Task<Results<Ok<QualificationDto>, NotFound, BadRequest<string>>> UpdateQualification(
        string id,
        [FromBody] UpdateQualificationCommand command,
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

    private static async Task<Results<NoContent, NotFound>> DeleteQualification(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteQualificationCommand { Id = id };
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
