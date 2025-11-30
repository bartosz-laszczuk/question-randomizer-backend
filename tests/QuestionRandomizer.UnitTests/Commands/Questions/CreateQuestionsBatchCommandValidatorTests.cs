namespace QuestionRandomizer.UnitTests.Commands.Questions;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestionsBatch;

/// <summary>
/// Unit tests for CreateQuestionsBatchCommandValidator
/// </summary>
public class CreateQuestionsBatchCommandValidatorTests
{
    private readonly CreateQuestionsBatchCommandValidator _validator;

    public CreateQuestionsBatchCommandValidatorTests()
    {
        _validator = new CreateQuestionsBatchCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new CreateQuestionRequest
                {
                    QuestionText = "What is CQRS?",
                    Answer = "CQRS stands for Command Query Responsibility Segregation...",
                    AnswerPl = "CQRS oznacza rozdzielenie odpowiedzialności komend i zapytań..."
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyQuestions_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Questions)
            .WithErrorMessage("At least one question is required");
    }

    [Fact]
    public void Validate_TooManyQuestions_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = Enumerable.Range(1, 101).Select(i => new CreateQuestionRequest
            {
                QuestionText = $"Question {i}",
                Answer = $"Answer {i}",
                AnswerPl = $"Odpowiedź {i}"
            }).ToList()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Questions)
            .WithErrorMessage("Maximum 100 questions can be created at once");
    }

    [Fact]
    public void Validate_Exactly100Questions_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = Enumerable.Range(1, 100).Select(i => new CreateQuestionRequest
            {
                QuestionText = $"Question {i}",
                Answer = $"Answer {i}",
                AnswerPl = $"Odpowiedź {i}"
            }).ToList()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Questions);
    }

    [Fact]
    public void Validate_EmptyQuestionText_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new CreateQuestionRequest
                {
                    QuestionText = "",
                    Answer = "Valid answer",
                    AnswerPl = "Poprawna odpowiedź"
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Questions[0].QuestionText")
            .WithErrorMessage("Question text is required");
    }

    [Fact]
    public void Validate_QuestionTextTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new CreateQuestionRequest
                {
                    QuestionText = new string('a', 1001),
                    Answer = "Valid answer",
                    AnswerPl = "Poprawna odpowiedź"
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Questions[0].QuestionText")
            .WithErrorMessage("Question text must not exceed 1000 characters");
    }

    [Fact]
    public void Validate_EmptyAnswer_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new CreateQuestionRequest
                {
                    QuestionText = "Valid question",
                    Answer = "",
                    AnswerPl = "Poprawna odpowiedź"
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Questions[0].Answer")
            .WithErrorMessage("Answer is required");
    }

    [Fact]
    public void Validate_AnswerTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new CreateQuestionRequest
                {
                    QuestionText = "Valid question",
                    Answer = new string('b', 5001),
                    AnswerPl = "Poprawna odpowiedź"
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Questions[0].Answer")
            .WithErrorMessage("Answer must not exceed 5000 characters");
    }

    [Fact]
    public void Validate_EmptyAnswerPl_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new CreateQuestionRequest
                {
                    QuestionText = "Valid question",
                    Answer = "Valid answer",
                    AnswerPl = ""
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Questions[0].AnswerPl")
            .WithErrorMessage("Polish answer is required");
    }

    [Fact]
    public void Validate_AnswerPlTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionsBatchCommand
        {
            Questions = new List<CreateQuestionRequest>
            {
                new CreateQuestionRequest
                {
                    QuestionText = "Valid question",
                    Answer = "Valid answer",
                    AnswerPl = new string('c', 5001)
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Questions[0].AnswerPl")
            .WithErrorMessage("Polish answer must not exceed 5000 characters");
    }
}
