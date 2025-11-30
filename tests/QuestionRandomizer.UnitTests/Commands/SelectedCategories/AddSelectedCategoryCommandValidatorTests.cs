namespace QuestionRandomizer.UnitTests.Commands.SelectedCategories;

using FluentValidation.TestHelper;
using QuestionRandomizer.Application.Commands.SelectedCategories.AddSelectedCategory;

/// <summary>
/// Unit tests for AddSelectedCategoryCommandValidator
/// </summary>
public class AddSelectedCategoryCommandValidatorTests
{
    private readonly AddSelectedCategoryCommandValidator _validator;

    public AddSelectedCategoryCommandValidatorTests()
    {
        _validator = new AddSelectedCategoryCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "rand123",
            CategoryId = "cat456",
            CategoryName = "Clean Architecture"
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
        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "",
            CategoryId = "cat456",
            CategoryName = "Clean Architecture"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RandomizationId)
            .WithErrorMessage("Randomization ID is required");
    }

    [Fact]
    public void Validate_EmptyCategoryId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "rand123",
            CategoryId = "",
            CategoryName = "Clean Architecture"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category ID is required");
    }

    [Fact]
    public void Validate_EmptyCategoryName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "rand123",
            CategoryId = "cat456",
            CategoryName = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryName)
            .WithErrorMessage("Category name is required");
    }

    [Fact]
    public void Validate_CategoryNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "rand123",
            CategoryId = "cat456",
            CategoryName = new string('a', 201) // 201 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryName)
            .WithErrorMessage("Category name must not exceed 200 characters");
    }

    [Fact]
    public void Validate_CategoryNameExactly200Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "rand123",
            CategoryId = "cat456",
            CategoryName = new string('a', 200) // Exactly 200 characters
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryName);
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new AddSelectedCategoryCommand
        {
            RandomizationId = "",
            CategoryId = "",
            CategoryName = new string('a', 201)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RandomizationId);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        result.ShouldHaveValidationErrorFor(x => x.CategoryName);
    }
}
