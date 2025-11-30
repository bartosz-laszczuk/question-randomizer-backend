namespace QuestionRandomizer.UnitTests.Commands.Questions;

using FluentAssertions;
using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestion;

/// <summary>
/// Unit tests for CreateQuestionCommandValidator
/// </summary>
public class CreateQuestionCommandValidatorTests
{
    private readonly CreateQuestionCommandValidator _validator;

    public CreateQuestionCommandValidatorTests()
    {
        _validator = new CreateQuestionCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "What is Clean Architecture?",
            Answer = "Clean Architecture is a software design philosophy...",
            AnswerPl = "Clean Architecture to filozofia projektowania oprogramowania..."
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyQuestionText_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuestionText)
            .WithErrorMessage("Question text is required");
    }

    [Fact]
    public void Validate_QuestionTextTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = new string('a', 1001), // 1001 characters
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuestionText)
            .WithErrorMessage("Question text must not exceed 1000 characters");
    }

    [Fact]
    public void Validate_QuestionTextExactly1000Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = new string('a', 1000), // Exactly 1000 characters
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QuestionText);
    }

    [Fact]
    public void Validate_EmptyAnswer_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "",
            AnswerPl = "Poprawna odpowiedź"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Answer)
            .WithErrorMessage("Answer is required");
    }

    [Fact]
    public void Validate_AnswerTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = new string('b', 5001), // 5001 characters
            AnswerPl = "Poprawna odpowiedź"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Answer)
            .WithErrorMessage("Answer must not exceed 5000 characters");
    }

    [Fact]
    public void Validate_AnswerExactly5000Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = new string('b', 5000), // Exactly 5000 characters
            AnswerPl = "Poprawna odpowiedź"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Answer);
    }

    [Fact]
    public void Validate_EmptyAnswerPl_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AnswerPl)
            .WithErrorMessage("Polish answer is required");
    }

    [Fact]
    public void Validate_AnswerPlTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = new string('c', 5001) // 5001 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AnswerPl)
            .WithErrorMessage("Polish answer must not exceed 5000 characters");
    }

    [Fact]
    public void Validate_AnswerPlExactly5000Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = new string('c', 5000) // Exactly 5000 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AnswerPl);
    }

    [Fact]
    public void Validate_CategoryIdTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            CategoryId = new string('d', 101) // 101 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category ID must not exceed 100 characters");
    }

    [Fact]
    public void Validate_CategoryIdExactly100Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            CategoryId = new string('d', 100) // Exactly 100 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Validate_NullCategoryId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            CategoryId = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Validate_QualificationIdTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            QualificationId = new string('e', 101) // 101 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QualificationId)
            .WithErrorMessage("Qualification ID must not exceed 100 characters");
    }

    [Fact]
    public void Validate_QualificationIdExactly100Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            QualificationId = new string('e', 100) // Exactly 100 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QualificationId);
    }

    [Fact]
    public void Validate_NullQualificationId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            QualificationId = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QualificationId);
    }

    [Fact]
    public void Validate_TagsWithin20Limit_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            Tags = Enumerable.Range(1, 20).Select(i => $"tag{i}").ToList() // Exactly 20 tags
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Validate_TagsExceeding20Limit_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            Tags = Enumerable.Range(1, 21).Select(i => $"tag{i}").ToList() // 21 tags
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Maximum 20 tags allowed");
    }

    [Fact]
    public void Validate_NullTags_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            Tags = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Validate_EmptyTagsList_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "Valid question",
            Answer = "Valid answer",
            AnswerPl = "Poprawna odpowiedź",
            Tags = new List<string>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new CreateQuestionCommand
        {
            QuestionText = "", // Empty
            Answer = "", // Empty
            AnswerPl = "", // Empty
            CategoryId = new string('x', 101), // Too long
            QualificationId = new string('y', 101), // Too long
            Tags = Enumerable.Range(1, 21).Select(i => $"tag{i}").ToList() // Too many
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuestionText);
        result.ShouldHaveValidationErrorFor(x => x.Answer);
        result.ShouldHaveValidationErrorFor(x => x.AnswerPl);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        result.ShouldHaveValidationErrorFor(x => x.QualificationId);
        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }
}
