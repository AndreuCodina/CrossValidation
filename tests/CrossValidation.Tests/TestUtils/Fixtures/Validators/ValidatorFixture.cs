using System.Linq;
using CrossValidation.Errors;

namespace CrossValidation.Tests.TestUtils.Fixtures.Validators;

public class ValidatorFixture
{
    public bool AllFieldsAreAddedAsPlaceholders(ICrossError error)
    {
        var placeholderNamesAdded = error.PlaceholderValues!
            .Select(x => x.Key)
            .ToList();
        var areSpecificErrorFieldsAddedAsPlaceholders = error.GetFieldNames()
            .All(fieldName => placeholderNamesAdded.Contains(fieldName));
        return areSpecificErrorFieldsAddedAsPlaceholders;
    }
}