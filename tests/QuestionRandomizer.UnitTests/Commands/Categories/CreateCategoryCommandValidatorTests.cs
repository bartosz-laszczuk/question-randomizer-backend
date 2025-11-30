namespace QuestionRandomizer.UnitTests.Commands.Categories;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.Categories.CreateCategory;

/// <summary>
/// Unit tests for CreateCategoryCommandValidator
/// </summary>
public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator;

    public CreateCategoryCommandValidatorTests()
    {
        _validator = new CreateCategoryCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Clean Architecture"
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
        var command = new CreateCategoryCommand
        {
            Name = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Category name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = new string('a', 101) // 101 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Category name must not exceed 100 characters");
    }

    [Fact]
    public void Validate_NameExactly100Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = new string('a', 100) // Exactly 100 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
