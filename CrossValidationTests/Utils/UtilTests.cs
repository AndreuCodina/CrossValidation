using System;
using System.Collections.Generic;
using System.Linq;
using CrossValidation.Utils;
using CrossValidationTests.Builders;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Utils;

public class UtilTests
{
    [Fact]
    public void Field_selector_with_method_call_fails()
    {
        var model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {1})
            .Build();
        
        var action = () =>
            Util.GetFieldInformation(x => x.NullableIntList!.First(), model);
        action.ShouldThrow<InvalidOperationException>();
        
        action = () =>
            Util.GetFieldInformation(x => x.NullableIntList![0], model);
        action.ShouldThrow<InvalidOperationException>();
    }
    
    [Fact]
    public void Field_selector_to_null_field_returns_null_value()
    {
        var model = new ParentModelBuilder().Build();
        
        var result = Util.GetFieldInformation(x => x.NullableInt, model);
        
        result.Value.ShouldBeNull();
    }
    
    [Fact]
    public void Field_selector_to_same_model_returns_model()
    {
        var model = new ParentModelBuilder().Build();
        
        var result = Util.GetFieldInformation(x => x, model);
        
        result.Value.ShouldBe(model);
    }
    
    [Fact]
    public void Field_selector_to_child_model_returns_child_model()
    {
        var model = new ParentModelBuilder().Build();
        
        var result = Util.GetFieldInformation(x => x.NestedModel, model);

        result.Value.ShouldNotBeNull();
    }
}