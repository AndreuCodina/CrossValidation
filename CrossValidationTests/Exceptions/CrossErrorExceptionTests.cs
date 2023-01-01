using CrossValidation.Exceptions;
using CrossValidation.Results;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Exceptions;

public class CrossErrorExceptionTests
{
    [Fact]
    public void Retrieve_error_type()
    {
        var exception = new CrossException(new TestError.ErrorWithoutProperties());

        exception.Error.ShouldBeOfType<TestError.ErrorWithoutProperties>();
    }
    
    [Fact]
    public void Retrieve_error_message()
    {
        var exception = new CrossException(new TestError.ErrorWithoutProperties());
        exception.Message.ShouldBe("ErrorWithoutProperties { }");
        
        exception = new CrossException(new TestError.ErrorWithProperties("Property1Value", null));
        exception.Message.ShouldBe("ErrorWithProperties { Property1 = Property1Value, Property2 =  }");
        
        exception = new CrossException(new TestError.ErrorWithoutProperties(), "Custom message");
        exception.Message.ShouldBe("ErrorWithoutProperties { }. Custom message");
    }

    public record TestError : CrossError
    {
        public record ErrorWithoutProperties : TestError;
        public record ErrorWithProperties(string Property1, int? Property2) : TestError;
    }
}