using System;
using System.Collections.Generic;
using CrossValidationTests.Models;

namespace CrossValidationTests.Builders;

public class ParentModelBuilder
{
    private string? _nullableString;
    private NestedModel _nestedModel = new NestedModelBuilder().Build();
    private List<int>? _nullableIntList;
    private int? _nullableInt;

    public ParentModel Build()
    {
        var model = new ParentModel
        {
            String = "The string",
            NullableString = _nullableString,
            NullableDateTime = DateTime.UtcNow,
            NestedModel = _nestedModel,
            NullableIntList = _nullableIntList,
            NullableInt = _nullableInt,
            NestedEnum = NestedEnum.Red
        };

        return model;
    }

    public ParentModelBuilder WithNestedModel(Action<NestedModelBuilder> builder)
    {
        var dab = new NestedModelBuilder();
        builder(dab);
        _nestedModel = dab.Build();
        return this;
    }

    public ParentModelBuilder WithNullableString(string? nullableString)
    {
        _nullableString = nullableString;
        return this;
    }
    
    public ParentModelBuilder WithNullableIntList(List<int>? nullableIntList)
    {
        _nullableIntList = nullableIntList;
        return this;
    }
    
    public ParentModelBuilder WithNullableInt(int? nullableInt)
    {
        _nullableInt = nullableInt;
        return this;
    }
}