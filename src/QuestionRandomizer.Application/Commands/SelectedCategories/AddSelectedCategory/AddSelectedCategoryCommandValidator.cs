namespace QuestionRandomizer.Application.Commands.SelectedCategories.AddSelectedCategory;

using FluentValidation;

/// <summary>
/// Validator for AddSelectedCategoryCommand
/// </summary>
public class AddSelectedCategoryCommandValidator : AbstractValidator<AddSelectedCategoryCommand>
{
    public AddSelectedCategoryCommandValidator()
    {
        RuleFor(x => x.RandomizationId)
            .NotEmpty().WithMessage("Randomization ID is required");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required");

        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(200).WithMessage("Category name must not exceed 200 characters");
    }
}
