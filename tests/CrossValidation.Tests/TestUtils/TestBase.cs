using System.Globalization;

namespace CrossValidation.Tests.TestUtils;

public class TestBase
{
    protected TestBase()
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCultureCode);
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCultureCode);
    }
}