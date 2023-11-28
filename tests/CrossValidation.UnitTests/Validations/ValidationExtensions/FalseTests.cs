using Common.Tests;
using CrossValidation.ErrorResources;
using CrossValidation.Exceptions;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class FalseTests : TestBase
{
    private readonly ParentModel _model;

    public FalseTests()
    {
        _model = new ParentModelBuilder()
            .Build();
    }
    
    [Fact]
    public void Not_fail_when_field_value_is_false()
    {
        var action = () => Validate.Field(_model.FalseBoolean)
            .False();

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Fail_when_field_value_is_not_false()
    {
        var action = () => Validate.Field(_model.TrueBoolean)
            .False();

        var exception = action.ShouldThrow<CommonException.FalseBooleanException>();
        exception.Code.ShouldBe(nameof(ErrorResource.Generic));
    }
}