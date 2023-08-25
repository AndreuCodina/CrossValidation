using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Fixtures;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

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
    public void Throw_exception_when_the_validation_fails()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Nullable string")
            .Build();
        
        var action = () => Validate.Field(_model.NullableString)
            .Null();

        var exception = action.ShouldThrow<CommonException.NullException>();
        exception.Code.ShouldBe(nameof(ErrorResource.Null));
    }
}