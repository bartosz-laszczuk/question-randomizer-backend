namespace QuestionRandomizer.UnitTests.Commands.Qualifications;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualification;

/// <summary>
/// Unit tests for CreateQualificationCommandValidator
/// </summary>
public class CreateQualificationCommandValidatorTests
{
    private readonly CreateQualificationCommandValidator _validator;

    public CreateQualificationCommandValidatorTests()
    {
        _validator = new CreateQualificationCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateQualificationCommand
        {
            Name = "INF.02"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQualificationCommand
        {
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
        var command = new CreateQualificationCommand
        {
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
        var command = new CreateQualificationCommand
        {
            Name = new string('a', 100) // Exactly 100 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
