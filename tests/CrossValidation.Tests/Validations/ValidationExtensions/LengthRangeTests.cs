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

public class LengthRangeTests : TestBase
{
    private ParentModel _model;

    public LengthRangeTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        var action = () => Validate.Field(_model.String)
            .LengthRange(_model.String.Length + 1, _model.String.Length);

        var error = action.ShouldThrowCrossError<CommonCrossError.LengthRange>();
        error.Code.ShouldBe(nameof(ErrorResource.LengthRange));
    }
}