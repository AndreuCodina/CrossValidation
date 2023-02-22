using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Models;
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
        var action = () => Validate.That(_model.NullableString)
            .NotNull();

        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }
}