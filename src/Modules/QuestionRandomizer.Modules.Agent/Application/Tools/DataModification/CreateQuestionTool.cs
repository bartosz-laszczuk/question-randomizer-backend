namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataModification;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Entities;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for creating a new interview question
/// </summary>
public class CreateQuestionTool : AgentToolBase<CreateQuestionInput>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateQuestionTool(
        IQuestionRepository questionRepository,
        ICategoryRepository categoryRepository,
        ILogger<CreateQuestionTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
        _categoryRepository = categoryRepository;
    }

    public override string Name => "create_question";

    public override string Description =>
        @"Creates a new interview question.

Use this tool to:
- Add new questions to the user's question bank
- Create questions based on user requests
- Populate the database with sample questions

Requires: questionText, answer, answerPl (Polish translation)
Optional: categoryId, qualificationId, tags";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""questionText"": {
      ""type"": ""string"",
      ""description"": ""The question text in English""
    },
    ""answer"": {
      ""type"": ""string"",
      ""description"": ""The answer text in English""
    },
    ""answerPl"": {
      ""type"": ""string"",
      ""description"": ""The answer text in Polish""
    },
    ""categoryId"": {
      ""type"": ""string"",
      ""description"": ""Optional: Category ID to assign to this question""
    },
    ""qualificationId"": {
      ""type"": ""string"",
      ""description"": ""Optional: Qualification ID to assign to this question""
    },
    ""tags"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" },
      ""description"": ""Optional: Array of tags for this question""
    }
  },
  ""required"": [""questionText"", ""answer"", ""answerPl""]
}";

    protected override CreateQuestionInput ValidateAndParse(JsonElement input)
    {
        var validator = new CreateQuestionInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        CreateQuestionInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        // Resolve category name if categoryId is provided
        string? categoryName = null;
        if (!string.IsNullOrEmpty(input.CategoryId))
        {
            var category = await _categoryRepository.GetByIdAsync(
                input.CategoryId,
                userId,
                cancellationToken);

            if (category != null)
            {
                categoryName = category.Name;
            }
        }

        // Create the question
        var question = new Question
        {
            QuestionText = input.QuestionText,
            Answer = input.Answer,
            AnswerPl = input.AnswerPl,
            CategoryId = input.CategoryId,
            CategoryName = categoryName,
            QualificationId = input.QualificationId,
            Tags = input.Tags,
            UserId = userId, // CRITICAL: Set userId for security
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _questionRepository.CreateAsync(question, cancellationToken);

        return new
        {
            success = true,
            message = "Question created successfully",
            question = new
            {
                id = created.Id,
                questionText = created.QuestionText,
                answer = created.Answer,
                answerPl = created.AnswerPl,
                categoryId = created.CategoryId,
                categoryName = created.CategoryName,
                qualificationId = created.QualificationId,
                tags = created.Tags,
                createdAt = created.CreatedAt
            }
        };
    }
}

/// <summary>
/// Input parameters for CreateQuestionTool
/// </summary>
public class CreateQuestionInput
{
    public string QuestionText { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string AnswerPl { get; set; } = string.Empty;
    public string? CategoryId { get; set; }
    public string? QualificationId { get; set; }
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Validator for CreateQuestionInput
/// </summary>
public class CreateQuestionInputValidator : AbstractValidator<CreateQuestionInput>
{
    public CreateQuestionInputValidator()
    {
        RuleFor(x => x.QuestionText)
            .NotEmpty()
            .WithMessage("Question text is required")
            .MaximumLength(1000)
            .WithMessage("Question text cannot exceed 1000 characters");

        RuleFor(x => x.Answer)
            .NotEmpty()
            .WithMessage("Answer is required")
            .MaximumLength(5000)
            .WithMessage("Answer cannot exceed 5000 characters");

        RuleFor(x => x.AnswerPl)
            .NotEmpty()
            .WithMessage("Polish answer is required")
            .MaximumLength(5000)
            .WithMessage("Polish answer cannot exceed 5000 characters");
    }
}
