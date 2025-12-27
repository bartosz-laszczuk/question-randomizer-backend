namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataModification;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for updating multiple questions at once
/// </summary>
public class BatchUpdateQuestionsTool : AgentToolBase<BatchUpdateQuestionsInput>
{
    private readonly IQuestionRepository _questionRepository;

    public BatchUpdateQuestionsTool(
        IQuestionRepository questionRepository,
        ILogger<BatchUpdateQuestionsTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
    }

    public override string Name => "batch_update_questions";

    public override string Description =>
        @"Updates multiple questions at once with the same field values.

Use this tool to:
- Assign the same category to multiple questions
- Bulk update qualification assignments
- Add tags to multiple questions at once

Provide a list of question IDs and the fields to update.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""questionIds"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" },
      ""description"": ""Array of question IDs to update""
    },
    ""categoryId"": {
      ""type"": ""string"",
      ""description"": ""Optional: Category ID to assign to all questions""
    },
    ""qualificationId"": {
      ""type"": ""string"",
      ""description"": ""Optional: Qualification ID to assign to all questions""
    },
    ""tags"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" },
      ""description"": ""Optional: Tags to add to all questions""
    }
  },
  ""required"": [""questionIds""]
}";

    protected override BatchUpdateQuestionsInput ValidateAndParse(JsonElement input)
    {
        var validator = new BatchUpdateQuestionsInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        BatchUpdateQuestionsInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        var updatedCount = 0;
        var failedIds = new List<string>();

        foreach (var questionId in input.QuestionIds)
        {
            var question = await _questionRepository.GetByIdAsync(
                questionId,
                userId,
                cancellationToken);

            if (question == null)
            {
                failedIds.Add(questionId);
                continue;
            }

            if (input.CategoryId != null)
                question.CategoryId = input.CategoryId;
            if (input.QualificationId != null)
                question.QualificationId = input.QualificationId;
            if (input.Tags != null)
                question.Tags = input.Tags;

            question.UpdatedAt = DateTime.UtcNow;

            var updated = await _questionRepository.UpdateAsync(question, cancellationToken);
            if (updated)
                updatedCount++;
            else
                failedIds.Add(questionId);
        }

        return new
        {
            success = updatedCount > 0,
            updatedCount,
            totalRequested = input.QuestionIds.Count,
            failedCount = failedIds.Count,
            failedIds = failedIds.Count > 0 ? failedIds : null,
            message = $"Updated {updatedCount} of {input.QuestionIds.Count} questions"
        };
    }
}

public class BatchUpdateQuestionsInput
{
    public List<string> QuestionIds { get; set; } = new();
    public string? CategoryId { get; set; }
    public string? QualificationId { get; set; }
    public List<string>? Tags { get; set; }
}

public class BatchUpdateQuestionsInputValidator : AbstractValidator<BatchUpdateQuestionsInput>
{
    public BatchUpdateQuestionsInputValidator()
    {
        RuleFor(x => x.QuestionIds)
            .NotEmpty()
            .WithMessage("At least one question ID is required")
            .Must(ids => ids.Count <= 100)
            .WithMessage("Cannot update more than 100 questions at once");
    }
}
