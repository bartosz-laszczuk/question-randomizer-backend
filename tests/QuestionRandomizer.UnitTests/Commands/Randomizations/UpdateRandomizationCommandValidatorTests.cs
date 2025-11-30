namespace QuestionRandomizer.UnitTests.Commands.Randomizations;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Randomizations.UpdateRandomization;

/// <summary>
/// Unit tests for UpdateRandomizationCommandValidator
/// </summary>
public class UpdateRandomizationCommandValidatorTests
{
    private readonly UpdateRandomizationCommandValidator _validator;

    public UpdateRandomizationCommandValidatorTests()
    {
        _validator = new UpdateRandomizationCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_OngoingStatus_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateRandomizationCommand
        {
            Id = "rand123",
            Status = "Ongoing"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommand_CompletedStatus_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateRandomizationCommand
        {
            Id = "rand123",
            Status = "Completed"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateRandomizationCommand
        {
            Id = "",
            Status = "Ongoing"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Randomization ID is required");
    }

    [Fact]
    public void Validate_EmptyStatus_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateRandomizationCommand
        {
            Id = "rand123",
            Status = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status is required");
    }

    [Fact]
    public void Validate_InvalidStatus_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateRandomizationCommand
        {
            Id = "rand123",
            Status = "InvalidStatus"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status must be 'Ongoing' or 'Completed'");
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new UpdateRandomizationCommand
        {
            Id = "",
            Status = "Invalid"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
