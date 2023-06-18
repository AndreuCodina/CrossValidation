using CrossValidation.Tests.TestUtils.Models;

namespace CrossValidation.Tests.TestUtils.Fixtures.Validators;

public record ParentModelValidator : ModelValidator<ParentModel>
{
    public override void CreateValidations()
    {
    }
}