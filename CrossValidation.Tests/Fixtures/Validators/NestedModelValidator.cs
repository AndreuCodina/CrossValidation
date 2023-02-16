using CrossValidation.Tests.Models;

namespace CrossValidation.Tests.Fixtures.Validators;

public record NestedModelValidator : ModelValidator<NestedModel>
{
    public override void CreateValidations(NestedModel model)
    {
    }
}