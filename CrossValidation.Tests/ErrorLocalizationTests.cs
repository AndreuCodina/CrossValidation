using CrossValidation.Extensions;
using CrossValidation.Resources;
using CrossValidation.Rules;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class ErrorLocalizationTests
{
    [Fact]
    public void Validator_error_is_localized()
    {
        var action = () => Rule<string?>.CreateFromField(() => null, RuleState.Valid)
            .NotNull();

        var error = action.ShouldThrowValidationError();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }
}