using CrossValidation.Tests.Models;

namespace CrossValidation.Tests.Fixtures.Validators;

public record ParentModelValidator : ModelValidator<ParentModel>
{
    public override void CreateValidations(ParentModel model)
    {
    }
}