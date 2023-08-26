using Common.Tests;
using CrossValidation.Validators.PredicateValidators;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Validators.PredicateValidators;

public class BooleanValidatorTests : TestBase
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