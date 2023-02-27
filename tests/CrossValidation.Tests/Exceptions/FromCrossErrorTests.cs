using System;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Exceptions;

public class FromCrossErrorTests : TestBase
{
    private ParentModel _model;

    public FromCrossErrorTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Throw_exception_implementing_ICrossErrorToException()
    {
        var parametrizedExceptionAction = () => Validate<ExceptionFromError>.Field(_model.NullableInt)
            .NotNull();
        var crossArgumentException = parametrizedExceptionAction.ShouldThrow<ExceptionFromError>();
        crossArgumentException.Message.ShouldBe("Expected message");
        
        var defaultExceptionAction = () => Validate.Field(_model.NullableInt)
            .NotNull();
        var crossException = defaultExceptionAction.ShouldThrow<CrossException>();
        crossException.Error.Message.ShouldBe(ErrorResource.NotNull);
        
        var withErrorAction = () => Validate<ExceptionFromError>.That(_model.NullableInt)
            .WithError(new CrossError())
            .NotNull();
        withErrorAction.ShouldThrow<ExceptionFromError>();
        
        var valueObjectAction = () => Validate<ExceptionFromError>.That(_model.Int)
            .Instance(ValueObject.Create);
        valueObjectAction.ShouldThrow<ExceptionFromError>();

        Action exceptionAction = () => throw new CrossError().ToException();
        exceptionAction.ShouldThrow<CrossException>();
    }

    private record ValueObject(int Value)
    {
        public static ValueObject Create(int value)
        {
            Validate.That(value).Must(_ => false);
            return new(value);
        }
    }
    
    private class ExceptionFromError : Exception, ICrossErrorToException
    {
        private ExceptionFromError(string message) : base(message)
        {
        }
        
        public static Exception FromCrossError(ICrossError error)
        {
            return new ExceptionFromError("Expected message");
        }
    }
}