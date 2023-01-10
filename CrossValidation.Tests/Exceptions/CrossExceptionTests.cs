using CrossValidation.Errors;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Exceptions;

public class CrossExceptionTests
{
    [Fact]
    public void Add_error()
    {
        var exception = new TestError.ErrorWithoutProperties().ToException();

        exception.Error.ShouldBeOfType<TestError.ErrorWithoutProperties>();
    }
    
    [Fact]
    public void Add_message()
    {
        var exception = new TestError.ErrorWithoutProperties().ToException();
        exception.Message.ShouldBe("ErrorWithoutProperties { }");
        
        exception = new TestError.ErrorWithProperties("Property1Value", null).ToException();
        exception.Message.ShouldBe("ErrorWithProperties { Property1 = Property1Value, Property2 =  }");
        
        exception = new TestError.ErrorWithoutProperties().ToException("Custom message");
        exception.Message.ShouldBe("ErrorWithoutProperties { }. Custom message");
    }
    
    [Fact]
    public void Add_message_description()
    {
        var expectedMessageDescription = "Expected message description";
        var exception = new TestError.ErrorWithoutProperties().ToException(expectedMessageDescription);
        exception.MessageDescription.ShouldBe(expectedMessageDescription);
        exception.Message.ShouldContain(expectedMessageDescription);
    }

    private record TestError : Error
    {
        public record ErrorWithoutProperties : TestError;
        public record ErrorWithProperties(string Property1, int? Property2) : TestError;
    }
}