using CrossValidation.Exceptions;
using CrossValidation.Resources;
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
    public void Return_exception_when_the_validation_fails()
    {
        var action = () => Validate.Field(_model.String)
            .MinimumLength(_model.String.Length + 1);

        var exception = action.ShouldThrow<CommonCrossException.MinimumLength>();
        exception.Code.ShouldBe(nameof(ErrorResource.MinimumLength));
    }
}