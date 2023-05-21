using System;
using System.Collections.Generic;
using CrossValidation.Tests.TestUtils.Models;

namespace CrossValidation.Tests.TestUtils.Builders;

public class ParentModelBuilder
{
    private string? _nullableString;
    private NestedModel _nestedModel = new NestedModelBuilder().Build();
    private NestedModel? _nullableNestedModel;
    private List<int> _intList = new() {1, 2, 3};
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
            NullableNestedModel = _nullableNestedModel,
            IntList = _intList,
            NullableIntList = _nullableIntList,
            IntListList = new()
            {
                new() {1, 2, 1},
                new() {4, 1, 6}
            },
            StringList = new List<string> {"1", "2", "3"},
            NullableStringList = null,
            Int = 1,
            NullableInt = _nullableInt,
            Enum = ParentModelEnum.Case1
        };

        return model;
    }

    public ParentModelBuilder WithNestedModel(Action<NestedModelBuilder>? action = null)
    {
        var builder = new NestedModelBuilder();
        action?.Invoke(builder);
        _nestedModel = builder.Build();
        return this;
    }
    
    public ParentModelBuilder WithNullableNestedModel(Action<NestedModelBuilder>? action = null)
    {
        var builder = new NestedModelBuilder();
        action?.Invoke(builder);
        _nullableNestedModel = builder.Build();
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