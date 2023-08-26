using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests;

public class ErrorLocalizationTests : TestBase
{
    private readonly ParentModel _model;

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