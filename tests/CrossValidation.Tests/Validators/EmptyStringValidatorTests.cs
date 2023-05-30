using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validators;

public class EmptyStringValidatorTests : TestBase
{
    private ParentModel _model;

    public EmptyStringValidatorTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Validate_empty_string()
    {
        var value = "";
        var validator = new EmptyStringValidator(value);

        var isValid = validator.IsValid();

        isValid.ShouldBeTrue();
    }
    
    [Fact]
    public void Validate_empty_string_fails()
    {
        var validator = new NullValidator<string>(_model.String);

        var isValid = validator.IsValid();

        isValid.ShouldBeFalse();
    }
}