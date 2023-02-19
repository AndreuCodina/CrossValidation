using System;
using System.Collections.Generic;
using CrossValidation.Tests.Models;

namespace CrossValidation.Tests.Builders;

public class ParentModelBuilder
{
    private string? _nullableString;
    private NestedModel _nestedModel = new NestedModelBuilder().Build();
    private List<int> _intList = new List<int> {1, 2, 3};
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
            NullableNestedModel = null,
            IntList = _intList,
            NullableIntList = _nullableIntList,
            StringList = new List<string> {"1", "2", "3"},
            NullableStringList = null,
            Int = 1,
            NullableInt = _nullableInt,
            Enum = ParentModelEnum.Red
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
    
    public ParentModelBuilder WithIntList(List<int> intList)
    {
        _intList = intList;
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