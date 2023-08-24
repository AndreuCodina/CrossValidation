using System.Globalization;

namespace CrossValidation.UnitTests.TestUtils;

public class TestBase
{
    protected TestBase()
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCultureCode);
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCultureCode);
    }
}