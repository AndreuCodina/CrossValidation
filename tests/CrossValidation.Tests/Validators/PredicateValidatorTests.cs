using CrossValidation.Tests.TestUtils;
using CrossValidation.Validators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators;

public class PredicateValidatorTests : TestBase
{
    [Fact]
    public void Validate_predicate()
    {
        var validator = new BooleanPredicateValidator(() => true);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Validate_predicate_fails()
    {
        var validator = new BooleanPredicateValidator(() => false);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}