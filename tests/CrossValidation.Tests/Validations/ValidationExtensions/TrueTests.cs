using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

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