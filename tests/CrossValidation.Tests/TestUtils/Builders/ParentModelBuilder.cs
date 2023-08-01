using CrossValidation.Tests.TestUtils.Models;

namespace CrossValidation.Tests.TestUtils.Builders;

public class ParentModelBuilder
{
    private string? _nullableString;
    private NestedModel _nestedModel = new NestedModelBuilder().Build();
    private NestedModel? _nullableNestedModel;
    private List<int> _intList = new() {1, 2, 3};
    private List<int>? _nullableIntList;
    private List<List<int>> _intListList = new()
    {
        new() {1, 2, 1},
        new() {4, 1, 6}
    };
    private int? _nullableInt;
    private string _string = "The string";

    public ParentModel Build()
    {
        var model = new ParentModel
        {
            String = _string,
            NullableString = _nullableString,
            NullableDateTime = DateTime.UtcNow,
            NestedModel = _nestedModel,
            NullableNestedModel = _nullableNestedModel,
            IntList = _intList,
            NullableIntList = _nullableIntList,
            IntListList = _intListList,
            StringList = new List<string> {"1", "2", "3"},
            NullableStringList = null,
            NestedModelList = new()
            {
                new NestedModelBuilder().Build(),
                new NestedModelBuilder().Build()
            },
            Int = 1,
            NullableInt = _nullableInt,
            Enum = ParentModelEnum.Case1,
            TrueBoolean = true,
            FalseBoolean = false
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

    public ParentModelBuilder WithNullableString(string? value)
    {
        _nullableString = value;
        return this;
    }
    
    public ParentModelBuilder WithIntList(List<int> value)
    {
        _intList = value;
        return this;
    }
    
    public ParentModelBuilder WithIntListList(List<List<int>> value)
    {
        _intListList = value;
        return this;
    }

    public ParentModelBuilder WithNullableIntList(List<int>? value)
    {
        _nullableIntList = value;
        return this;
    }
    
    public ParentModelBuilder WithNullableInt(int? value)
    {
        _nullableInt = value;
        return this;
    }

    public ParentModelBuilder WithString(string value)
    {
        _string = value;
        return this;
    }
}