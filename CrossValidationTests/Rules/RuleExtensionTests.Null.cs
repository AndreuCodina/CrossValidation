using CrossValidation;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules;

public class RuleExtensionTests_Null : IClassFixture<Fixture>
{
    private readonly Fixture _fixture;
    private ParentModel _model;

    public RuleExtensionTests_Null(Fixture fixture)
    {
        _fixture = fixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Null_works_with_nullable_value_types()
    {
        var action = () => Validate.That(_model.NullableInt)
            .Null()
            .Must(_fixture.BeValid);
        
        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Null_works_with_nullable_reference_types()
    {
        var action = () => Validate.That(_model.NullableString)
            .Null()
            .Must(_fixture.BeValid);
        
        action.ShouldNotThrow();
    }
}