namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataAnalysis;

using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Tool for analyzing question difficulty/complexity
/// </summary>
public class AnalyzeQuestionDifficultyTool : AgentToolBase<AnalyzeQuestionDifficultyInput>
{
    private readonly IQuestionRepository _questionRepository;

    public AnalyzeQuestionDifficultyTool(
        IQuestionRepository questionRepository,
        ILogger<AnalyzeQuestionDifficultyTool> logger) : base(logger)
    {
        _questionRepository = questionRepository;
    }

    public override string Name => "analyze_question_difficulty";

    public override string Description =>
        @"Analyzes the difficulty/complexity of questions.

Use this tool to:
- Assess question difficulty level
- Identify overly simple or complex questions
- Get recommendations for difficulty improvements

Returns difficulty scores and analysis for questions.";

    public override string InputSchemaJson => @"{
  ""type"": ""object"",
  ""properties"": {
    ""questionId"": {
      ""type"": ""string"",
      ""description"": ""Optional: Analyze a specific question by ID""
    },
    ""categoryId"": {
      ""type"": ""string"",
      ""description"": ""Optional: Analyze all questions in a category""
    },
    ""limit"": {
      ""type"": ""number"",
      ""description"": ""Maximum number of questions to analyze (default: 20)"",
      ""minimum"": 1,
      ""maximum"": 100
    }
  }
}";

    protected override AnalyzeQuestionDifficultyInput ValidateAndParse(JsonElement input)
    {
        var validator = new AnalyzeQuestionDifficultyInputValidator();
        return DeserializeAndValidate(input, validator);
    }

    protected override async Task<object> ExecuteToolAsync(
        AnalyzeQuestionDifficultyInput input,
        string userId,
        CancellationToken cancellationToken)
    {
        List<Questions.Domain.Entities.Question> questionsToAnalyze;

        if (!string.IsNullOrEmpty(input.QuestionId))
        {
            var question = await _questionRepository.GetByIdAsync(
                input.QuestionId,
                userId,
                cancellationToken);

            questionsToAnalyze = question != null ? new List<Questions.Domain.Entities.Question> { question } : new();
        }
        else if (!string.IsNullOrEmpty(input.CategoryId))
        {
            questionsToAnalyze = await _questionRepository.GetByCategoryIdAsync(
                input.CategoryId,
                userId,
                cancellationToken);
        }
        else
        {
            questionsToAnalyze = await _questionRepository.GetByUserIdAsync(
                userId,
                cancellationToken);
        }

        questionsToAnalyze = questionsToAnalyze
            .Where(q => q.IsActive)
            .Take(input.Limit ?? 20)
            .ToList();

        var analyses = questionsToAnalyze.Select(q =>
        {
            var analysis = AnalyzeComplexity(q.QuestionText, q.Answer);
            return new
            {
                questionId = q.Id,
                questionText = q.QuestionText.Substring(0, Math.Min(100, q.QuestionText.Length)) + "...",
                difficultyScore = analysis.Score,
                level = analysis.Level,
                factors = analysis.Factors
            };
        }).ToList();

        return new
        {
            success = true,
            count = analyses.Count,
            analyses
        };
    }

    private static (double Score, string Level, List<string> Factors) AnalyzeComplexity(
        string questionText,
        string answer)
    {
        var factors = new List<string>();
        double score = 0.0;

        // Factor 1: Question length
        var wordCount = questionText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount < 10)
        {
            score += 1;
            factors.Add("Short question (simple)");
        }
        else if (wordCount > 30)
        {
            score += 3;
            factors.Add("Long question (complex)");
        }
        else
        {
            score += 2;
            factors.Add("Medium length question");
        }

        // Factor 2: Technical terms
        var technicalTerms = new[] { "algorithm", "complexity", "implement", "design", "optimize",
            "architecture", "pattern", "async", "concurrent", "thread", "memory", "performance" };
        var technicalCount = technicalTerms.Count(term =>
            questionText.ToLowerInvariant().Contains(term) ||
            answer.ToLowerInvariant().Contains(term));

        if (technicalCount >= 3)
        {
            score += 3;
            factors.Add($"High technical term count ({technicalCount})");
        }
        else if (technicalCount > 0)
        {
            score += 2;
            factors.Add($"Some technical terms ({technicalCount})");
        }

        // Factor 3: Answer complexity
        var answerWordCount = answer.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (answerWordCount > 100)
        {
            score += 3;
            factors.Add("Detailed answer required");
        }
        else if (answerWordCount > 50)
        {
            score += 2;
            factors.Add("Moderate answer length");
        }
        else
        {
            score += 1;
            factors.Add("Short answer");
        }

        // Factor 4: Code blocks in answer
        if (Regex.IsMatch(answer, @"```[\s\S]*```") || answer.Contains("function") || answer.Contains("class"))
        {
            score += 2;
            factors.Add("Contains code examples");
        }

        // Normalize score to 0-10
        score = Math.Min(score / 1.1, 10);

        string level = score switch
        {
            < 3 => "Easy",
            < 6 => "Medium",
            < 8 => "Hard",
            _ => "Very Hard"
        };

        return (Math.Round(score, 1), level, factors);
    }
}

public class AnalyzeQuestionDifficultyInput
{
    public string? QuestionId { get; set; }
    public string? CategoryId { get; set; }
    public int? Limit { get; set; }
}

public class AnalyzeQuestionDifficultyInputValidator : AbstractValidator<AnalyzeQuestionDifficultyInput>
{
    public AnalyzeQuestionDifficultyInputValidator()
    {
        When(x => x.Limit.HasValue, () =>
        {
            RuleFor(x => x.Limit!.Value)
                .InclusiveBetween(1, 100)
                .WithMessage("Limit must be between 1 and 100");
        });
    }
}
