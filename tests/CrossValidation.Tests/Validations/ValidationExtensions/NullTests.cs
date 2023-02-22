using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class NullTests :
    TestBase,
    IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public NullTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Null_works_with_nullable_value_types()
    {
        var action = () => Validate.That(_model.NullableInt)
            .Null()
            .Must(_commonFixture.BeValid);
        
        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Null_works_with_nullable_reference_types()
    {
        var action = () => Validate.That(_model.NullableString)
            .Null()
            .Must(_commonFixture.BeValid);
        
        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        var value = "123";

        var action = () => Validate.That(value)
            .Null();

        var error = action.ShouldThrowCrossError<CommonCrossError.Null>();
        error.Code.ShouldBe(nameof(ErrorResource.Null));
    }
}