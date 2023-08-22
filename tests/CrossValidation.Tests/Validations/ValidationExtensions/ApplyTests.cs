using CrossValidation.Exceptions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class ApplyTests : TestBase
{
    private readonly ParentModel _model;

    public ApplyTests()
    {
        _model = new ParentModelBuilder()
            .Build();
    }
    
    [Fact]
    public void Apply_validations_and_return_void()
    {
        void ApplyValidations(IValidation<string?> validation)
        {
            validation
                .NotNull()
                .MinimumLength(10);
        }
        
        var action = () => Validate.Field(_model.NullableString)
            .Apply(ApplyValidations);

        action.Should()
            .Throw<CommonException.NotNullException>();
    }
    
    [Fact]
    public void Apply_validations_and_return_resulting_validation()
    {
        IValidation<string> ApplyValidations(IValidation<string?> validation)
        {
            return validation
                .NotNull()
                .MinimumLength(10);
        }
        
        var action = () => Validate.Field(_model.NullableString)
            .Apply(ApplyValidations);

        action.Should()
            .Throw<CommonException.NotNullException>();
    }
}