namespace QuestionRandomizer.UnitTests.Commands.Messages;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Messages.AddMessage;

/// <summary>
/// Unit tests for AddMessageCommandValidator
/// </summary>
public class AddMessageCommandValidatorTests
{
    private readonly AddMessageCommandValidator _validator;

    public AddMessageCommandValidatorTests()
    {
        _validator = new AddMessageCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_UserRole_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new AddMessageCommand
        {
            ConversationId = "conv123",
            Role = "user",
            Content = "What is Clean Architecture?"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommand_AssistantRole_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new AddMessageCommand
        {
            ConversationId = "conv123",
            Role = "assistant",
            Content = "Clean Architecture is a software design philosophy..."
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyConversationId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddMessageCommand
        {
            ConversationId = "",
            Role = "user",
            Content = "Valid content"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConversationId)
            .WithErrorMessage("Conversation ID is required");
    }

    [Fact]
    public void Validate_EmptyRole_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddMessageCommand
        {
            ConversationId = "conv123",
            Role = "",
            Content = "Valid content"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("Role is required");
    }

    [Fact]
    public void Validate_InvalidRole_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddMessageCommand
        {
            ConversationId = "conv123",
            Role = "invalid",
            Content = "Valid content"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("Role must be 'user' or 'assistant'");
    }

    [Fact]
    public void Validate_EmptyContent_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddMessageCommand
        {
            ConversationId = "conv123",
            Role = "user",
            Content = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Message content is required");
    }

    [Fact]
    public void Validate_ContentTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddMessageCommand
        {
            ConversationId = "conv123",
            Role = "user",
            Content = new string('a', 10001) // 10001 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content must not exceed 10000 characters");
    }

    [Fact]
    public void Validate_ContentExactly10000Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new AddMessageCommand
        {
            ConversationId = "conv123",
            Role = "user",
            Content = new string('a', 10000) // Exactly 10000 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new AddMessageCommand
        {
            ConversationId = "",
            Role = "invalid",
            Content = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConversationId);
        result.ShouldHaveValidationErrorFor(x => x.Role);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }
}
