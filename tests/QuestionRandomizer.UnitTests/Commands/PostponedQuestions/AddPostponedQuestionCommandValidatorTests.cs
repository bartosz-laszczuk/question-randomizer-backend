namespace QuestionRandomizer.UnitTests.Commands.PostponedQuestions;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.PostponedQuestions.AddPostponedQuestion;

/// <summary>
/// Unit tests for AddPostponedQuestionCommandValidator
/// </summary>
public class AddPostponedQuestionCommandValidatorTests
{
    private readonly AddPostponedQuestionCommandValidator _validator;

    public AddPostponedQuestionCommandValidatorTests()
    {
        _validator = new AddPostponedQuestionCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new AddPostponedQuestionCommand
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
        var command = new AddPostponedQuestionCommand
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
        var command = new AddPostponedQuestionCommand
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
        var command = new AddPostponedQuestionCommand
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
