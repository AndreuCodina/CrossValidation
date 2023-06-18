using CrossValidation.Tests.TestUtils.Models;

namespace CrossValidation.Tests.TestUtils.Fixtures.Validators;

public record NestedModelValidator : ModelValidator<NestedModel>
{
    public override void CreateValidations()
    {
    }
}