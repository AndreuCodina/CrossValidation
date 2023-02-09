using System.Linq;
using CrossValidation.Errors;

namespace CrossValidation.Tests.Fixtures.Validators;

public class ValidatorFixture
{
    public bool AllFieldsAreAddedAsPlaceholders(ICrossError error)
    {
        var placeholderNamesAdded = error.PlaceholderValues!
            .Select(x => x.Key)
            .ToList();
        var areSpecificErrorFieldsAddedAsPlaceholders = error.GetFieldNames()
            .All(fieldName => placeholderNamesAdded.Contains(fieldName));
        var errorContainsCommonPlaceholders =
            ErrorContainsPlaceholder(error, nameof(CrossError.FieldName))
            && ErrorContainsPlaceholder(error, nameof(CrossError.FieldDisplayName))
            && ErrorContainsPlaceholder(error, nameof(CrossError.FieldValue));
        return areSpecificErrorFieldsAddedAsPlaceholders && errorContainsCommonPlaceholders;
    }

    private bool ErrorContainsPlaceholder(ICrossError error, string placeholderName)
    {
        return error.PlaceholderValues!
            .Select(x => x.Key)
            .Contains(placeholderName);
    }
}