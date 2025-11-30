namespace QuestionRandomizer.UnitTests.Commands.Qualifications;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Qualifications.UpdateQualification;

/// <summary>
/// Unit tests for UpdateQualificationCommandValidator
/// </summary>
public class UpdateQualificationCommandValidatorTests
{
    private readonly UpdateQualificationCommandValidator _validator;

    public UpdateQualificationCommandValidatorTests()
    {
        _validator = new UpdateQualificationCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateQualificationCommand
        {
            Id = "qual123",
            Name = "Updated Qualification"
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
        var command = new UpdateQualificationCommand
        {
            Id = "",
            Name = "Valid name"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Qualification ID is required");
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateQualificationCommand
        {
            Id = "qual123",
            Name = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Qualification name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateQualificationCommand
        {
            Id = "qual123",
            Name = new string('a', 101) // 101 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Qualification name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_NameExactly100Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateQualificationCommand
        {
            Id = "qual123",
            Name = new string('a', 100) // Exactly 100 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new UpdateQualificationCommand
        {
            Id = "",
            Name = new string('a', 101)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
