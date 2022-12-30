using CrossValidation;
using CrossValidationTests.Models;

namespace CrossValidationTests.Fixtures.Validators;

public record ParentModelValidator : ModelValidator<ParentModel>
{
    public override void CreateRules(ParentModel model)
    {
    }
}