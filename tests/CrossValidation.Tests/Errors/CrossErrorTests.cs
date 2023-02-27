using System;
using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Fixtures.Validators;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Errors;

public class CrossErrorTests :
    TestBase,
    IClassFixture<CommonFixture>,
    IClassFixture<ValidatorFixture>
{
    private readonly CommonFixture _commonFixture;
    private readonly ValidatorFixture _validatorFixture;

    public CrossErrorTests(CommonFixture commonFixture, ValidatorFixture validatorFixture)
    {
        _commonFixture = commonFixture;
        _validatorFixture = validatorFixture;
    }
    
    [Fact]
    public void Add_all_specific_error_fields_as_placeholders()
    {
        var action = () => Validate.That(true)
            .WithError(new ErrorWithAllFieldsAddedAsPlaceholders("Alex", 30))
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowCrossError<ErrorWithAllFieldsAddedAsPlaceholders>();

        _validatorFixture.AllFieldsAreAddedAsPlaceholders(error).ShouldBeTrue();
    }
    
    [Fact]
    public void Do_not_add_all_specific_error_fields_as_placeholders_fails()
    {
        var action = () => Validate.That(true)
            .WithError(new ErrorWithNotAllFieldsAddedAsPlaceholders("Alex", 30))
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowCrossError<ErrorWithNotAllFieldsAddedAsPlaceholders>();

        _validatorFixture.AllFieldsAreAddedAsPlaceholders(error).ShouldBeFalse();
    }
    
    [Fact]
    public void Add_null_placeholders_as_empty_strings()
    {
        var action = () => Validate.That(true)
            .WithError(new ErrorWithNullFields(null, null))
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowCrossError<ErrorWithNullFields>();

        error.PlaceholderValues![nameof(CrossError.FieldName)].ShouldBe("");
        error.PlaceholderValues![nameof(ErrorWithNullFields.Name)].ShouldBe("");
        error.PlaceholderValues![nameof(ErrorWithNullFields.Age)].ShouldBe("");
    }
    
    [Fact]
    public void Throw_cross_exception_with_code_customizes_message()
    {
        Action action = () => throw new CustomNotNull().ToException();

        var error = action.ShouldThrowCrossError<CustomNotNull>();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
        error.Message.ShouldBe(ErrorResource.NotNull);
    }

    private record ErrorWithAllFieldsAddedAsPlaceholders(string Name, int Age) : CrossError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Name);
            AddPlaceholderValue(Age);
            base.AddPlaceholderValues();
        }
    }
    
    private record ErrorWithNotAllFieldsAddedAsPlaceholders(string Name, int Age) : CrossError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Name);
            base.AddPlaceholderValues();
        }
    }
    
    private record ErrorWithNullFields(string? Name, int? Age) : CrossError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Name);
            AddPlaceholderValue(Age);
            base.AddPlaceholderValues();
        }
    }
    
    private record CustomNotNull() : CodeCrossError(nameof(ErrorResource.NotNull));
}