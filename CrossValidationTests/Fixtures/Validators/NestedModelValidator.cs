using CrossValidation;
using CrossValidationTests.Models;

namespace CrossValidationTests.Fixtures.Validators;

public record NestedModelValidator : ModelValidator<NestedModel>
{
    public override void CreateRules(NestedModel model)
    {
    }
}