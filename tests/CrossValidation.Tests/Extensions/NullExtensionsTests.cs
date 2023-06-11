using CrossValidation.Extensions;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Extensions;

public class NullExtensionsTests
{
    private ParentModel _model;

    public NullExtensionsTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Return_null_when_mapping_null_value_type()
    {
        var result = _model.NullableInt.Map(ValueObjectFromValueType.Create);
        
        result.ShouldBeNull();
    }
    
    [Fact]
    public void Return_value_object_when_mapping_null_value_type()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        
        var result = _model.NullableInt.Map(ValueObjectFromValueType.Create);
        
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public void Return_null_when_mapping_null_reference_type()
    {
        var result = _model.NullableString.Map(ValueObjectFromReferenceType.Create);
        
        result.ShouldBeNull();
    }
    
    [Fact]
    public void Return_value_object_when_mapping_null_reference_type()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("")
            .Build();
        
        var result = _model.NullableString.Map(ValueObjectFromReferenceType.Create);
        
        result.ShouldNotBeNull();
    }
    
    private record ValueObjectFromValueType(int Value)
    {
        public static ValueObjectFromValueType Create(int value)
        {
            return new(value);
        }
    }
    
    private record ValueObjectFromReferenceType(string Value)
    {
        public static ValueObjectFromReferenceType Create(string value)
        {
            return new(value);
        }
    }
}