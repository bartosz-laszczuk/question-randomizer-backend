namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataRetrieval;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for retrieving a specific question by its ID
/// </summary>
public class GetQuestionByIdTool : AgentToolBase<GetQuestionByIdInput>
{
    private readonly IQuestionRepository _questionRepository;

    public GetQuestionByIdTool(
        IQuestionRepository questionRepository,
        ILogger<GetQuestionByIdTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
    }

    public override string Name => "get_question_by_id";

    public override string Description =>
        @"Retrieves a specific interview question by its ID.

Use this tool to:
- Get details of a specific question
- Verify a question exists
- Check question content before updating

Returns the full question details if found.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""questionId"": {
      ""type"": ""string"",
      ""description"": ""The ID of the question to retrieve""
    }
  },
  ""required"": [""questionId""]
}";

    protected override GetQuestionByIdInput ValidateAndParse(JsonElement input)
    {
        var validator = new GetQuestionByIdInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        GetQuestionByIdInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        // CRITICAL: Always verify userId for security
        var question = await _questionRepository.GetByIdAsync(
            input.QuestionId,
            userId,
            cancellationToken);

        if (question == null)
        {
            return new
            {
                success = false,
                error = $"Question with ID '{input.QuestionId}' not found or unauthorized"
            };
        }

        return new
        {
            success = true,
            question = new
            {
                id = question.Id,
                questionText = question.QuestionText,
                answer = question.Answer,
                answerPl = question.AnswerPl,
                categoryId = question.CategoryId,
                categoryName = question.CategoryName,
                qualificationId = question.QualificationId,
                qualificationName = question.QualificationName,
                isActive = question.IsActive,
                tags = question.Tags,
                createdAt = question.CreatedAt,
                updatedAt = question.UpdatedAt
            }
        };
    }
}

/// <summary>
/// Input parameters for GetQuestionByIdTool
/// </summary>
public class GetQuestionByIdInput
{
    public string QuestionId { get; set; } = string.Empty;
}

/// <summary>
/// Validator for GetQuestionByIdInput
/// </summary>
public class GetQuestionByIdInputValidator : AbstractValidator<GetQuestionByIdInput>
{
    public GetQuestionByIdInputValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage("Question ID is required");
    }
}
