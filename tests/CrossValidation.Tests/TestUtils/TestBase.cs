using System.Globalization;
using System.Threading;

namespace CrossValidation.Tests.TestUtils;

public class TestBase
{
    protected TestBase()
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCultureCode);
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCultureCode);
    }
}