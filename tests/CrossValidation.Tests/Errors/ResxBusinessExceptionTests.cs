using CrossValidation.Exceptions;
using CrossValidation.WebApplication.Resources;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Errors;

public partial class ResxBusinessExceptionTests
{
    [Fact]
    public void Create_code_with_implicit_code_name()
    {
        var expectedCode = nameof(ErrorResource1.Code_With_Stops);
        
        var error = new CodeWithStops();
    
        error.Code.ShouldBe(expectedCode);
    }

    public partial class CodeWithStops() : ResxBusinessException(ErrorResource1.Code_With_Stops);
}