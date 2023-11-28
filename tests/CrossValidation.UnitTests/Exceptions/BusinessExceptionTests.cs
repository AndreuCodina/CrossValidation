using Common.Tests;
using CrossValidation.ErrorResources;
using CrossValidation.Exceptions;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Exceptions;

public class BusinessExceptionTests : TestBase
{
    [Fact]
    public void Throw_cross_exception_with_code_customizes_message()
    {
        Action action = () => throw new BusinessException(code: nameof(ErrorResource.NotNull));

        var exception = action.ShouldThrow<BusinessException>();
        exception.Code.ShouldBe(nameof(ErrorResource.NotNull));
        exception.Message.ShouldBe(ErrorResource.NotNull);
    }
}