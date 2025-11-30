namespace QuestionRandomizer.UnitTests.Commands.UsedQuestions;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.UsedQuestions.AddUsedQuestion;

/// <summary>
/// Unit tests for AddUsedQuestionCommandValidator
/// </summary>
public class AddUsedQuestionCommandValidatorTests
{
    private readonly AddUsedQuestionCommandValidator _validator;

    public AddUsedQuestionCommandValidatorTests()
    {
        _validator = new AddUsedQuestionCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new AddUsedQuestionCommand
        {
            RandomizationId = "rand123",
            QuestionId = "quest456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyRandomizationId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddUsedQuestionCommand
        {
            RandomizationId = "",
            QuestionId = "quest456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RandomizationId)
            .WithErrorMessage("Randomization ID is required");
    }

    [Fact]
    public void Validate_EmptyQuestionId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddUsedQuestionCommand
        {
            RandomizationId = "rand123",
            QuestionId = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuestionId)
            .WithErrorMessage("Question ID is required");
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new AddUsedQuestionCommand
        {
            RandomizationId = "",
            QuestionId = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RandomizationId);
        result.ShouldHaveValidationErrorFor(x => x.QuestionId);
    }
}
