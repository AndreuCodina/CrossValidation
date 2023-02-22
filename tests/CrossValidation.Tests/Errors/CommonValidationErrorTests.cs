using System.Collections.Generic;
using CrossValidation.Errors;
using CrossValidation.Tests.Fixtures.Validators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Errors;

public class CommonValidationErrorTests :
    TestBase,
    IClassFixture<ValidatorFixture>
{
    private readonly ValidatorFixture _validatorFixture;

    public CommonValidationErrorTests(ValidatorFixture validatorFixture)
    {
        _validatorFixture = validatorFixture;
    }

    [Fact]
    public void Common_errors_add_their_placeholders()
    {
        var errors = new List<CommonCrossError>
        {
            new CommonCrossError.NotNull(),
            new CommonCrossError.Null(),
            new CommonCrossError.GreaterThan<int>(1),
            new CommonCrossError.Enum(),
            new CommonCrossError.LengthRange(1, 1, 1),
            new CommonCrossError.MinimumLength(1, 1),
            new CommonCrossError.Predicate()
        };
        errors.ForEach(error =>
        {
            error.AddPlaceholderValues();
            _validatorFixture.AllFieldsAreAddedAsPlaceholders(error).ShouldBeTrue();
        });
    }
}