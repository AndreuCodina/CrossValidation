using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Fixtures.Validators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Errors;

public class CompleteCrossErrorTests :
    TestBase,
    IClassFixture<CommonFixture>,
    IClassFixture<ValidatorFixture>
{
    private readonly CommonFixture _commonFixture;
    private readonly ValidatorFixture _validatorFixture;

    public CompleteCrossErrorTests(CommonFixture commonFixture, ValidatorFixture validatorFixture)
    {
        _commonFixture = commonFixture;
        _validatorFixture = validatorFixture;
    }

    [Fact]
    public void Not_add_all_specific_error_fields_as_placeholders_fails()
    {
        var action = () => Validate.That(true)
            .WithError(new ErrorWithNotAllFieldsAddedAsPlaceholders("Alex", 30))
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowCrossError<ErrorWithNotAllFieldsAddedAsPlaceholders>();

        _validatorFixture.AllFieldsAreAddedAsPlaceholders(error).ShouldBeFalse();
    }

    [Fact]
    public void Throw_cross_exception_with_code_customizes_message()
    {
        Action action = () => throw new CustomNotNull().ToException();

        var error = action.ShouldThrowCrossError<CustomNotNull>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }

    private record ErrorWithAllFieldsAddedAsPlaceholders(string Name, int Age) : CompleteCrossError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Name);
            AddPlaceholderValue(Age);
        }
    }
    
    private record ErrorWithNotAllFieldsAddedAsPlaceholders(string Name, int Age) : CompleteCrossError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Name);
        }
    }
    
    private record ErrorWithNullFields(string? Name, int? Age) : CompleteCrossError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Name);
            AddPlaceholderValue(Age);
        }
    }
    
    private record CustomNotNull() : CodeCrossError(ErrorResource.NotNull);
}