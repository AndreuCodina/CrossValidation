using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class NotEmptyTests : TestBase
{
    private ParentModel _model;

    public NotEmptyTests()
    {
        _model = new ParentModelBuilder()
            .Build();
    }

    [Fact]
    public void Not_fail_when_field_value_is_not_empty_string()
    {
        var action = () => Validate.Field(_model.String)
            .NotEmpty();

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Fail_when_field_value_is_empty_string()
    {
        _model = new ParentModelBuilder()
            .WithString("")
            .Build();
        
        var action = () => Validate.Field(_model.String)
            .NotEmpty();

        var error = action.ShouldThrowCrossError<CommonCrossError.NotEmptyString>();
        error.Code.ShouldBe(nameof(ErrorResource.NotEmptyString));
    }
    
    [Fact]
    public void Not_fail_when_field_value_is_not_empty_collection()
    {
        var action = () => Validate.Field(_model.IntList)
            .NotEmpty();
        
        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Fail_when_field_value_is_empty_collection()
    {
        _model = new ParentModelBuilder()
            .WithIntList(new())
            .Build();
        
        var action = () => Validate.Field(_model.IntList)
            .NotEmpty();

        var error = action.ShouldThrowCrossError<CommonCrossError.NotEmptyCollection>();
        error.Code.ShouldBe(nameof(ErrorResource.NotEmptyCollection));
    }
}