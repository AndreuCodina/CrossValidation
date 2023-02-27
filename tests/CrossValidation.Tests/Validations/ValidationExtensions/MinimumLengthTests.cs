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

public class MinimumLengthTests : TestBase
{
    private ParentModel _model;

    public MinimumLengthTests()
    {
        _model = new ParentModelBuilder()
            .Build();
    }
    
    [Fact]
    public void Return_error_when_the_validation_fails()
    {
        var action = () => Validate.Field(_model.String)
            .MinimumLength(_model.String.Length + 1);

        var error = action.ShouldThrowCrossError<CommonCrossError.MinimumLength>();
        error.Code.ShouldBe(nameof(ErrorResource.MinimumLength));
    }
}