using System.Linq;
using CrossValidation.Errors;

namespace CrossValidationTests.Fixtures.Validators;

public class ValidatorFixture
{
    public bool AllFieldsAreAddedAsPlaceholders(ICrossValidationError error)
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
}