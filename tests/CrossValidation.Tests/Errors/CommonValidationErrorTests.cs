using CrossValidation.Errors;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Fixtures.Validators;
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
        var error = new CommonCrossError.LengthRange(1, 1, 1);
        var action = () => Validate.That(1)
            .WithError(error)
            .Must(x => false);
        
        var exception = action.ShouldThrowCrossError();
        
        _validatorFixture.AllFieldsAreAddedAsPlaceholders(exception).ShouldBeTrue();
    }
}