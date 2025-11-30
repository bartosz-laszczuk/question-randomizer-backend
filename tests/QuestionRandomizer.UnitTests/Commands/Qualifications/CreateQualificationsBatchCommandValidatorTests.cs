namespace QuestionRandomizer.UnitTests.Commands.Qualifications;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Qualifications.CreateQualificationsBatch;

/// <summary>
/// Unit tests for CreateQualificationsBatchCommandValidator
/// </summary>
public class CreateQualificationsBatchCommandValidatorTests
{
    private readonly CreateQualificationsBatchCommandValidator _validator;

    public CreateQualificationsBatchCommandValidatorTests()
    {
        _validator = new CreateQualificationsBatchCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string> { "INF.02", "INF.03", "E.12" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyQualificationNames_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QualificationNames)
            .WithErrorMessage("At least one qualification name is required");
    }

    [Fact]
    public void Validate_TooManyQualificationNames_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = Enumerable.Range(1, 101).Select(i => $"Qualification {i}").ToList() // 101 qualifications
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QualificationNames)
            .WithErrorMessage("Maximum 100 qualifications can be created at once");
    }

    [Fact]
    public void Validate_Exactly100QualificationNames_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = Enumerable.Range(1, 100).Select(i => $"Qualification {i}").ToList() // Exactly 100 qualifications
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QualificationNames);
    }

    [Fact]
    public void Validate_EmptyQualificationNameInList_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string> { "INF.02", "", "E.12" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("QualificationNames[1]")
            .WithErrorMessage("Qualification name cannot be empty");
    }

    [Fact]
    public void Validate_QualificationNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string> { "INF.02", new string('a', 101), "E.12" }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("QualificationNames[1]")
            .WithErrorMessage("Qualification name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_QualificationNameExactly100Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateQualificationsBatchCommand
        {
            QualificationNames = new List<string> { new string('a', 100) }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QualificationNames);
    }
}
