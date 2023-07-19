using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Exceptions;

public class BusinessExceptionTests : TestBase
{
    [Fact]
    public void Throw_cross_exception_with_code_customizes_message()
    {
        Action action = () => throw new BusinessException(code: nameof(ErrorResource.NotNull));

        var error = action.ShouldThrowCrossError<BusinessException>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }
}