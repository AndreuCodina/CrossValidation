using System.Globalization;
using System.Threading;

namespace CrossValidation.Tests.TestUtils;

public class TestBase
{
    protected TestBase()
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCulture);
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(CrossValidationOptions.DefaultCulture);
    }
}