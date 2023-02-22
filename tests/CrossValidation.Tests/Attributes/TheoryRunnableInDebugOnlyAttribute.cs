using System.Diagnostics;
using Xunit;

namespace CrossValidation.Tests.Attributes;

public class TheoryRunnableInDebugOnlyAttribute : TheoryAttribute
{
    public TheoryRunnableInDebugOnlyAttribute()
    {
        if (!Debugger.IsAttached)
        {
            Skip = "Only running in interactive mode";
        }
    }
}