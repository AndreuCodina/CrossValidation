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
        var errors = new List<CommonCodeValidationError>
        {
            new CommonCodeValidationError.NotNull(),
            new CommonCodeValidationError.Null(),
            new CommonCodeValidationError.GreaterThan<int>(1),
            new CommonCodeValidationError.Enum(),
            new CommonCodeValidationError.LengthRange(1, 1, 1),
            new CommonCodeValidationError.MinimumLength(1, 1),
            new CommonCodeValidationError.Predicate(),
        };
        errors.ForEach(error =>
        {
            error.AddPlaceholderValues();
            _validatorFixture.AllFieldsAreAddedAsPlaceholders(error).ShouldBeTrue();
        });
    }
}