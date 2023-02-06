using System.Collections.Generic;
using CrossValidation.Errors;
using CrossValidation.Tests.Fixtures.Validators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Errors;

public class CommonValidationErrorTests : IClassFixture<ValidatorFixture>
{
    private readonly ValidatorFixture _validatorFixture;

    public CommonValidationErrorTests(ValidatorFixture validatorFixture)
    {
        _validatorFixture = validatorFixture;
    }

    [Fact]
    public void All_common_errors_add_their_placeholders()
    {
        var errors = new List<CommonValidationError>
        {
            new CommonValidationError.NotNull(),
            new CommonValidationError.Null(),
            new CommonValidationError.GreaterThan<int>(1),
            new CommonValidationError.Enum(),
            new CommonValidationError.LengthRange(1, 1, 1),
            new CommonValidationError.MinimumLength(1, 1),
            new CommonValidationError.Predicate(),
        };
        errors.ForEach(error =>
        {
            error.AddPlaceholderValues();
            _validatorFixture.AllFieldsAreAddedAsPlaceholders(error).ShouldBeTrue();
        });
    }
}