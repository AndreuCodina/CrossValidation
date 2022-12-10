using System;
using System.Linq;
using CrossValidation.Utils;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Utils;

public class UtilTests
{
    [Fact]
    public void Method_call_in_field_selector_fails()
    {
        var model = new CreateOrderModelBuilder().Build();
        
        var action = () =>
            Util.GetFieldInformation(x => x.ColorIds.First(), model);
        action.ShouldThrow<InvalidOperationException>();
        
        action = () =>
            Util.GetFieldInformation(x => x.ColorIds[0], model);
        action.ShouldThrow<InvalidOperationException>();
    }
}