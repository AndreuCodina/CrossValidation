using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class LengthRangeTests : TestBase
{
    private ParentModel _model;

    public LengthRangeTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Not_fail_when_is_out_of_range()
    {
        var action = () => Validate.Field(_model.String)
            .LengthRange(_model.String.Length, _model.String.Length);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Fail_when_is_out_of_range()
    {
        var action = () => Validate.Field(_model.String)
            .LengthRange(_model.String.Length + 1, _model.String.Length);

        var exception = action.ShouldThrow<CommonCrossException.LengthRange>();
        
        exception.Code.ShouldBe(nameof(ErrorResource.LengthRange));
    }
}