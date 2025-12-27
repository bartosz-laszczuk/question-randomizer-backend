namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataModification;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Entities;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for creating a new qualification
/// </summary>
public class CreateQualificationTool : AgentToolBase<CreateQualificationInput>
{
    private readonly IQualificationRepository _qualificationRepository;

    public CreateQualificationTool(
        IQualificationRepository qualificationRepository,
        ILogger<CreateQualificationTool> logger) : base(logger)
    {
        _qualificationRepository = qualificationRepository;
    }

    public override string Name => "create_qualification";

    public override string Description =>
        @"Creates a new job qualification.

Use this tool to:
- Add new qualifications/job roles
- Create qualifications for organizing questions by role

Returns the created qualification with its ID.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""name"": {
      ""type"": ""string"",
      ""description"": ""The qualification name (e.g., 'Frontend Developer', 'Backend Engineer')""
    }
  },
  ""required"": [""name""]
}";

    protected override CreateQualificationInput ValidateAndParse(JsonElement input)
    {
        var validator = new CreateQualificationInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        CreateQualificationInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        var qualification = new Qualification
        {
            Name = input.Name,
            UserId = userId,
            IsActive = true
        };

        var created = await _qualificationRepository.CreateAsync(qualification, cancellationToken);

        return new
        {
            success = true,
            message = "Qualification created successfully",
            qualification = new
            {
                id = created.Id,
                name = created.Name
            }
        };
    }
}

public class CreateQualificationInput
{
    public string Name { get; set; } = string.Empty;
}

public class CreateQualificationInputValidator : AbstractValidator<CreateQualificationInput>
{
    public CreateQualificationInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Qualification name is required")
            .MaximumLength(100)
            .WithMessage("Qualification name cannot exceed 100 characters");
    }
}
