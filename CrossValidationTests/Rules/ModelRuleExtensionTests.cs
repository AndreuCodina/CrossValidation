using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules;

public class ModelRuleExtensionTests : IClassFixture<ModelValidatorFixture>
{
    private readonly ModelValidatorFixture _modelValidatorFixture;
    private ParentModel _model;

    public ModelRuleExtensionTests(ModelValidatorFixture modelValidatorFixture)
    {
        _modelValidatorFixture = modelValidatorFixture;
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void NotNull_works_with_nullable_value_types()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableInt)
                .NotNull()
                .GreaterThan(_model.NullableInt!.Value - 1);
        });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void NotNull_works_with_nullable_reference_types()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("The string")
            .Build();
        
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .NotNull()
                .Must(_ => true);
        });
        var action = () => parentModelValidator.Validate(_model);
        action.ShouldNotThrow();

        parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .NotNull()
                .Must(_ => true);
        });
        action = () => parentModelValidator.Validate(_model);
        action.ShouldNotThrow();
    }

    [Fact]
    public void Null_works_with_nullable_value_types()
    {
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableInt)
                .Null()
                .Must(_ => true);
        });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Null_works_with_nullable_reference_types()
    {
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .Null()
                .Must(_ => true);
        });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldNotThrow();
    }
}