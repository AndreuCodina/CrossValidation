using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class TrueTests : TestBase
{
    private ParentModel _model;

    public TrueTests()
    {
        _model = new ParentModelBuilder()
            .Build();
    }
    
    [Fact]
    public void Not_fail_when_field_value_is_true()
    {
        var action = () => Validate.Field(_model.TrueBoolean)
            .True();

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Fail_when_field_value_is_not_true()
    {
        var action = () => Validate.Field(_model.FalseBoolean)
            .True();

        var exception = action.ShouldThrow<CommonException.TrueBooleanException>();
        
        exception.Code.ShouldBe(nameof(ErrorResource.Generic));
    }
}