using CrossValidation.Exceptions;

namespace CrossValidation.Tests.TestUtils.Fixtures.Validators;

public class ValidatorFixture
{
    public bool AllFieldsAreAddedAsPlaceholders(BusinessException exception)
    {
        var placeholderNamesAdded = exception.PlaceholderValues!
            .Select(x => x.Key)
            .ToList();
        var areSpecificErrorFieldsAddedAsPlaceholders = exception.GetFieldNames()
            .All(fieldName => placeholderNamesAdded.Contains(fieldName));
        return areSpecificErrorFieldsAddedAsPlaceholders;
    }
}