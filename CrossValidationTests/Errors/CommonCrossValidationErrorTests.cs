using System.Collections.Generic;
using CrossValidation;
using CrossValidationTests.Fixtures.Validators;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Errors;

public class CommonCrossValidationErrorTests : IClassFixture<ValidatorFixture>
{
    private readonly ValidatorFixture _validatorFixture;

    public CommonCrossValidationErrorTests(ValidatorFixture validatorFixture)
    {
        _validatorFixture = validatorFixture;
    }

    [Fact]
    public void All_common_errors_add_their_placeholders()
    {
        var errors = new List<CommonCrossValidationError>
        {
            new CommonCrossValidationError.NotNull(),
            new CommonCrossValidationError.Null(),
            new CommonCrossValidationError.GreaterThan<int>(1),
            new CommonCrossValidationError.Enum(),
            new CommonCrossValidationError.LengthRange(1, 1, 1),
            new CommonCrossValidationError.MinimumLength(1, 1),
            new CommonCrossValidationError.Predicate(),
        };
        errors.ForEach(error =>
        {
            error.AddPlaceholderValues();
            _validatorFixture.AllFieldsAreAddedAsPlaceholders(error).ShouldBeTrue();
        });
    }
}