namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataModification;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for updating question fields
/// </summary>
public class UpdateQuestionTool : AgentToolBase<UpdateQuestionInput>
{
    private readonly IQuestionRepository _questionRepository;

    public UpdateQuestionTool(
        IQuestionRepository questionRepository,
        ILogger<UpdateQuestionTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
    }

    public override string Name => "update_question";

    public override string Description =>
        @"Updates fields of an existing question.

Use this tool to:
- Modify question text or answers
- Update tags
- Change qualification assignment

All fields are optional - only provided fields will be updated.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""questionId"": {
      ""type"": ""string"",
      ""description"": ""The ID of the question to update""
    },
    ""questionText"": {
      ""type"": ""string"",
      ""description"": ""Optional: New question text""
    },
    ""answer"": {
      ""type"": ""string"",
      ""description"": ""Optional: New answer text""
    },
    ""answerPl"": {
      ""type"": ""string"",
      ""description"": ""Optional: New Polish answer text""
    },
    ""qualificationId"": {
      ""type"": ""string"",
      ""description"": ""Optional: New qualification ID""
    },
    ""tags"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" },
      ""description"": ""Optional: New tags array""
    }
  },
  ""required"": [""questionId""]
}";

    protected override UpdateQuestionInput ValidateAndParse(JsonElement input)
    {
        var validator = new UpdateQuestionInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        UpdateQuestionInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            input.QuestionId,
            userId,
            cancellationToken);

        if (question == null)
        {
            return new
            {
                success = false,
                error = $"Question with ID '{input.QuestionId}' not found"
            };
        }

        if (!string.IsNullOrEmpty(input.QuestionText))
            question.QuestionText = input.QuestionText;
        if (!string.IsNullOrEmpty(input.Answer))
            question.Answer = input.Answer;
        if (!string.IsNullOrEmpty(input.AnswerPl))
            question.AnswerPl = input.AnswerPl;
        if (input.QualificationId != null)
            question.QualificationId = input.QualificationId;
        if (input.Tags != null)
            question.Tags = input.Tags;

        question.UpdatedAt = DateTime.UtcNow;

        var updated = await _questionRepository.UpdateAsync(question, cancellationToken);

        return new
        {
            success = updated,
            message = updated ? "Question updated successfully" : "Update failed",
            question = new
            {
                id = question.Id,
                questionText = question.QuestionText,
                updatedAt = question.UpdatedAt
            }
        };
    }
}

public class UpdateQuestionInput
{
    public string QuestionId { get; set; } = string.Empty;
    public string? QuestionText { get; set; }
    public string? Answer { get; set; }
    public string? AnswerPl { get; set; }
    public string? QualificationId { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateQuestionInputValidator : AbstractValidator<UpdateQuestionInput>
{
    public UpdateQuestionInputValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage("Question ID is required");
    }
}
