using System;
using System.Collections.Generic;
using CrossValidation.Extensions;
using CrossValidation.Rules;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Rules.RuleExtensions;

public class ForEachTests : IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public ForEachTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Execute_validators_for_all_item_collection()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 100, 90, 80 })
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableIntList)
                .NotNull()
                .ForEach(x => x
                    .GreaterThan(1)
                    .GreaterThan(10));
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
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;
            validator.RuleFor(x => x.NullableIntList)
                .NotNull()
                .ForEach(x => x
                    .GreaterThan(0)
                    .GreaterThan(10));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrowValidationErrors();
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
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableIntList)
                .NotNull()
                .ForEach(x => x
                    .Transform(Convert.ToDouble)
                    .GreaterThan(10d));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowValidationError();
        error.FieldName.ShouldBe("NullableIntList[0]");
    }
    
    [Fact]
    public void Index_is_represented_in_field_name_when_iterate_collection()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {100, 1, 2})
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableIntList)
                .NotNull()
                .ForEach(x => x
                    .GreaterThan(10));
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowValidationError();
        error.FieldName.ShouldBe("NullableIntList[1]");
    }
}