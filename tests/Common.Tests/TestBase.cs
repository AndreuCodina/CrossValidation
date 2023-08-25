using System.Globalization;
using CrossValidation;

namespace Common.Tests;

public class TestBase
{
    protected TestBase()
    {
        // Restore options if they're modified in any test
        CrossValidationOptions.SetDefaultOptions();
        
        // To be able to check hardcoded error messages
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCultureCode);
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCultureCode);
    }
}