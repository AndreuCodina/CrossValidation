using System.Linq;
using CrossValidation;
using CrossValidation.Resources;
using CrossValidation.Rules;
using Shouldly;
using Xunit;

namespace CrossValidationTests;

public class ErrorLocalizationTests
{
    [Fact]
    public void Validator_error_is_localized()
    {
        var action = () => new InlineRule<string?>(null)
            .NotNull();

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Code.ShouldBe(nameof(ErrorResource.NotNull));
        exception.Errors.First().Message.ShouldBe(ErrorResource.NotNull);
    }
}