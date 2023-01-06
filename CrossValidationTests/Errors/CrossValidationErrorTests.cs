using System.Linq;
using CrossValidation;
using CrossValidation.Errors;
using CrossValidation.Extensions;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Fixtures.Validators;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Errors;

public class CrossValidationErrorTests :
    IClassFixture<CommonFixture>,
    IClassFixture<ValidatorFixture>
{
    private readonly CommonFixture _commonFixture;
    private readonly ValidatorFixture _validatorFixture;

    public CrossValidationErrorTests(CommonFixture commonFixture, ValidatorFixture validatorFixture)
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

        var error = action.ShouldThrowValidationError<ErrorWithAllFieldsAddedAsPlaceholders>();

        _validatorFixture.AllFieldsAreAddedAsPlaceholders(error).ShouldBeTrue();
    }
    
    [Fact]
    public void Do_not_add_all_specific_error_fields_as_placeholders_fails()
    {
        var action = () => Validate.That(true)
            .WithError(new ErrorWithNotAllFieldsAddedAsPlaceholders("Alex", 30))
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowValidationError<ErrorWithNotAllFieldsAddedAsPlaceholders>();

        _validatorFixture.AllFieldsAreAddedAsPlaceholders(error).ShouldBeFalse();
    }
    
    [Fact]
    public void Add_null_placeholders_as_empty_strings()
    {
        var action = () => Validate.That(true)
            .WithError(new ErrorWithNullFields(null, null))
            .Must(_commonFixture.NotBeValid);

        var error = action.ShouldThrowValidationError<ErrorWithNullFields>();

        error.PlaceholderValues![nameof(CrossValidationError.FieldName)].ShouldBe("");
        error.PlaceholderValues![nameof(ErrorWithNullFields.Name)].ShouldBe("");
        error.PlaceholderValues![nameof(ErrorWithNullFields.Age)].ShouldBe("");
    }
    
    private bool AllFieldsAreAddedAsPlaceholders(ICrossValidationError error)
    {
        var placeholderNamesAdded = error.PlaceholderValues!
            .Select(x => x.Key)
            .ToList();
        var areSpecificErrorFieldsAddedAsPlaceholders = error.GetFieldNames()
            .All(fieldName => placeholderNamesAdded.Contains(fieldName));
        var errorContainsCommonPlaceholders =
            ErrorContainsPlaceholder(error, nameof(CrossValidationError.FieldName))
            && ErrorContainsPlaceholder(error, nameof(CrossValidationError.FieldDisplayName))
            && ErrorContainsPlaceholder(error, nameof(CrossValidationError.FieldValue));
        return areSpecificErrorFieldsAddedAsPlaceholders && errorContainsCommonPlaceholders;
    }

    private bool ErrorContainsPlaceholder(ICrossValidationError error, string placeholderName)
    {
        return error.PlaceholderValues!
            .Select(x => x.Key)
            .Contains(placeholderName);
    }

    private record ErrorWithAllFieldsAddedAsPlaceholders(string Name, int Age) : CrossValidationError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Name);
            AddPlaceholderValue(Age);
            base.AddPlaceholderValues();
        }
    }
    
    private record ErrorWithNotAllFieldsAddedAsPlaceholders(string Name, int Age) : CrossValidationError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Name);
            base.AddPlaceholderValues();
        }
    }
    
    private record ErrorWithNullFields(string? Name, int? Age) : CrossValidationError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Name);
            AddPlaceholderValue(Age);
            base.AddPlaceholderValues();
        }
    }
}