using System.Linq;
using CrossValidation.Errors;

namespace CrossValidation.Tests.Fixtures.Validators;

public class ValidatorFixture
{
    public bool AllFieldsAreAddedAsPlaceholders(IValidationError error)
    {
        var placeholderNamesAdded = error.PlaceholderValues!
            .Select(x => x.Key)
            .ToList();
        var areSpecificErrorFieldsAddedAsPlaceholders = error.GetFieldNames()
            .All(fieldName => placeholderNamesAdded.Contains(fieldName));
        var errorContainsCommonPlaceholders =
            ErrorContainsPlaceholder(error, nameof(ValidationError.FieldName))
            && ErrorContainsPlaceholder(error, nameof(ValidationError.FieldDisplayName))
            && ErrorContainsPlaceholder(error, nameof(ValidationError.FieldValue));
        return areSpecificErrorFieldsAddedAsPlaceholders && errorContainsCommonPlaceholders;
    }

    private bool ErrorContainsPlaceholder(IValidationError error, string placeholderName)
    {
        return error.PlaceholderValues!
            .Select(x => x.Key)
            .Contains(placeholderName);
    }
}