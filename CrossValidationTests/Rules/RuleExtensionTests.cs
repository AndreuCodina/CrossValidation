using System;
using System.Collections.Generic;
using CrossValidation;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Rules;

public class RuleExtensionTests : IClassFixture<ModelValidatorFixture>
{
    private readonly ModelValidatorFixture _modelValidatorFixture;
    private ParentModel _model;

    public RuleExtensionTests(ModelValidatorFixture modelValidatorFixture)
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

        var action = () => Validate.That(_model.NullableInt)
            .NotNull(x => x
                .GreaterThan(_model.NullableInt!.Value - 1));
        action.ShouldNotThrow();
    }
    
    [Fact]
    public void NotNull_works_with_nullable_reference_types()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("The string")
            .Build();

        var action = () => Validate.That(_model.NullableString)
            .NotNull(x => x
                .Must(_ => true));
        action.ShouldNotThrow();
    }
    
        
    [Fact]
    public void NotNull_works_with_nullable_types_and_error_accumulation()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Value")
            .Build();
        
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;
            
            validator.RuleFor(x => x.NullableInt)
                .NotNull(x => x
                    .GreaterThan(-1));
            
            validator.RuleFor(x => x.NullableString)
                .NotNull(x => x
                    .Length(int.MaxValue, int.MaxValue));

                validator.RuleFor(x => x.NullableInt)
                    .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(3);
        errors[0].ShouldBeOfType<CommonCrossValidationError.NotNull>();
        errors[1].ShouldBeOfType<CommonCrossValidationError.LengthRange>();
        errors[2].ShouldBeOfType<CommonCrossValidationError.NotNull>();
    }

    [Fact]
    public void Null_works_with_nullable_value_types()
    {
        var action = () => Validate.That(_model.NullableInt)
            .Null()
            .Must(_ => true);
        
        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Null_works_with_nullable_reference_types()
    {
        var action = () => Validate.That(_model.NullableString)
            .Null()
            .Must(_ => true);
        
        action.ShouldNotThrow();
    }

    [Fact]
    public void Execute_validators_for_all_item_collection()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 100, 90, 80 })
            .Build();
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableIntList)
                .NotNull(x => x
                    .ForEach(x => x
                        .GreaterThan(1)
                        .GreaterThan(10)));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Execute_validators_for_all_item_collection_fails()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 100, 1, 90, 2 })
            .Build();
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;
            validator.RuleFor(x => x.NullableIntList)
                .NotNull(x => x
                    .ForEach(x => x
                        .GreaterThan(0)
                        .GreaterThan(10)));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(2);
        errors[0].FieldName.ShouldBe("NullableIntList[1]");
        errors[1].FieldName.ShouldBe("NullableIntList[3]");
    }
    
    [Fact]
    public void Keep_field_name_when_the_field_is_transformed_in_a_collection()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 1 })
            .Build();
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableIntList)
                .NotNull(x => x
                    .ForEach(x => x
                        .Transform(Convert.ToDouble)
                        .GreaterThan(10d)));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldName.ShouldBe("NullableIntList[0]");
    }
    
    [Fact]
    public void Index_is_represented_in_field_name_when_iterate_collection()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {100, 1, 2})
            .Build();
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableIntList)
                .NotNull(x => x
                    .ForEach(x => x
                        .GreaterThan(10)));
        });

        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        errors[0].FieldName.ShouldBe("NullableIntList[1]");
    }
}