using CrossValidation.Errors;
using CrossValidation.WebApplication.Resources;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Errors;

public class CodeCrossErrorTests
{
    [Fact]
    public void Create_code_with_implicit_code_name()
    {
        var expectedCode = nameof(ErrorResource1.Code_With_Stops);
        
        var error = new CodeCrossError(ErrorResource1.Code_With_Stops);

        error.Code.ShouldBe(expectedCode);
    }
}