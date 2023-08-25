using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

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

        var exception = action.ShouldThrow<CommonException.NotEmptyStringException>();
        exception.Code.ShouldBe(nameof(ErrorResource.NotEmptyString));
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

        var exception = action.ShouldThrow<CommonException.NotEmptyCollectionException>();
        exception.Code.ShouldBe(nameof(ErrorResource.NotEmptyCollection));
    }
}