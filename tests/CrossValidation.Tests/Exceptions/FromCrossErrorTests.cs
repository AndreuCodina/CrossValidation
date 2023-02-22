using System;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Models;
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
    public void Validate_throws_customized_exception()
    {
        var parametrizedExceptionAction = () => Validate<CrossArgumentException>.Field(_model.NullableInt)
            .NotNull();
        var crossArgumentException = parametrizedExceptionAction.ShouldThrow<CrossArgumentException>();
        crossArgumentException.Message.ShouldBe($"{nameof(_model.NullableInt)}: {ErrorResource.NotNull}");
        
        var defaultExceptionAction = () => Validate.Field(_model.NullableInt)
            .NotNull();
        var crossException = defaultExceptionAction.ShouldThrow<CrossException>();
        crossException.Error.Message.ShouldBe(ErrorResource.NotNull);
        
        var withErrorAction = () => Validate<CrossArgumentException>.That(_model.NullableInt)
            .WithError(new CrossError())
            .NotNull();
        withErrorAction.ShouldThrow<CrossArgumentException>();
        
        var valueObjectAction = () => Validate<CrossArgumentException>.That(_model.Int)
            .Instance(ValueObject.Create);
        valueObjectAction.ShouldThrow<CrossArgumentException>();

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
}