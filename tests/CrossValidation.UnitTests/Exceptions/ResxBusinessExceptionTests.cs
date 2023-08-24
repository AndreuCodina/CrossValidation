using CrossValidation.Exceptions;
using CrossValidation.WebApplication.Resources;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Exceptions;

public partial class ResxBusinessExceptionTests
{
    [Fact]
    public void Create_code_with_implicit_code_name()
    {
        var expectedCode = nameof(ErrorResource1.Code_With_Stops);
        
        var exception = new CodeWithStops();
    
        exception.Code.ShouldBe(expectedCode);
    }

    public partial class CodeWithStops() : ResxBusinessException(ErrorResource1.Code_With_Stops);
}