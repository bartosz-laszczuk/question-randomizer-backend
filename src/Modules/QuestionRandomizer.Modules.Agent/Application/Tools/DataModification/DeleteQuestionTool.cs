namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataModification;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for deleting a question (soft delete)
/// </summary>
public class DeleteQuestionTool : AgentToolBase<DeleteQuestionInput>
{
    private readonly IQuestionRepository _questionRepository;

    public DeleteQuestionTool(
        IQuestionRepository questionRepository,
        ILogger<DeleteQuestionTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
    }

    public override string Name => "delete_question";

    public override string Description =>
        @"Deletes a question (soft delete - sets IsActive to false).

Use this tool to:
- Remove unwanted questions
- Delete duplicate questions
- Clean up the question database

The question is not permanently deleted - it's marked as inactive.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""questionId"": {
      ""type"": ""string"",
      ""description"": ""The ID of the question to delete""
    }
  },
  ""required"": [""questionId""]
}";

    protected override DeleteQuestionInput ValidateAndParse(JsonElement input)
    {
        var validator = new DeleteQuestionInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        DeleteQuestionInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        var deleted = await _questionRepository.DeleteAsync(
            input.QuestionId,
            userId,
            cancellationToken);

        return new
        {
            success = deleted,
            message = deleted
                ? "Question deleted successfully"
                : "Question not found or already deleted"
        };
    }
}

public class DeleteQuestionInput
{
    public string QuestionId { get; set; } = string.Empty;
}

public class DeleteQuestionInputValidator : AbstractValidator<DeleteQuestionInput>
{
    public DeleteQuestionInputValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage("Question ID is required");
    }
}
