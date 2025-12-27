namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataRetrieval;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for retrieving all qualifications for a user
/// </summary>
public class GetQualificationsTool : AgentToolBase<GetQualificationsInput>
{
    private readonly IQualificationRepository _qualificationRepository;

    public GetQualificationsTool(
        IQualificationRepository qualificationRepository,
        ILogger<GetQualificationsTool> logger) : base(logger)
    {
        _qualificationRepository = qualificationRepository;
    }

    public override string Name => "get_qualifications";

    public override string Description =>
        @"Retrieves all job qualifications for the user.

Use this tool to:
- View all available qualifications
- Get qualification IDs and names
- Check what qualifications exist

Returns a list of qualifications with their IDs and names.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {}
}";

    protected override GetQualificationsInput ValidateAndParse(JsonElement input)
    {
        var validator = new GetQualificationsInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        GetQualificationsInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        var qualifications = await _qualificationRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        qualifications = qualifications.Where(q => q.IsActive).ToList();

        return new
        {
            success = true,
            count = qualifications.Count,
            qualifications = qualifications.Select(q => new
            {
                id = q.Id,
                name = q.Name,
                isActive = q.IsActive
            })
        };
    }
}

public class GetQualificationsInput { }

public class GetQualificationsInputValidator : AbstractValidator<GetQualificationsInput>
{
    public GetQualificationsInputValidator() { }
}
