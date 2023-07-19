using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class ErrorLocalizationTests : TestBase
{
    private ParentModel _model;

    public ErrorLocalizationTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Validator_error_is_localized()
    {
        var action = () => Validate.Field(_model.NullableString)
            .NotNull();

        var exception = action.ShouldThrow<BusinessException>();
        exception.Code.ShouldBe(nameof(ErrorResource.NotNull));
        exception.Message.ShouldBe(ErrorResource.NotNull);
    }
}