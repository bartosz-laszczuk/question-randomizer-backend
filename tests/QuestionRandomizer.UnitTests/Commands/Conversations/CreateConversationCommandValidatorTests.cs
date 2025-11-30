namespace QuestionRandomizer.UnitTests.Commands.Conversations;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Conversations.CreateConversation;

/// <summary>
/// Unit tests for CreateConversationCommandValidator
/// </summary>
public class CreateConversationCommandValidatorTests
{
    private readonly CreateConversationCommandValidator _validator;

    public CreateConversationCommandValidatorTests()
    {
        _validator = new CreateConversationCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateConversationCommand
        {
            Title = "Clean Architecture Discussion"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyTitle_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateConversationCommand
        {
            Title = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Conversation title is required");
    }

    [Fact]
    public void Validate_TitleTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateConversationCommand
        {
            Title = new string('a', 201) // 201 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters");
    }

    [Fact]
    public void Validate_TitleExactly200Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateConversationCommand
        {
            Title = new string('a', 200) // Exactly 200 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }
}
