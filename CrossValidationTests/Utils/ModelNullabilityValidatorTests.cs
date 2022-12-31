﻿using System.Collections.Generic;
using System.Linq;
using CrossValidation.Exceptions;
using CrossValidationTests.Builders;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Utils;

public class ModelNullabilityValidatorTests : IClassFixture<Fixture>
{
    private readonly Fixture _fixture;
    private ParentModel _model;

    public ModelNullabilityValidatorTests(Fixture fixture)
    {
        _fixture = fixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Non_nullable_field_with_null_fails()
    {
        SetPropertyValue(
            model: _model,
            propertyName: nameof(ParentModel.String),
            propertyValue: null);
        var parentModelValidator = _fixture.CreateParentModelValidator(_ => { });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrow<ModelFormatException>();
    }
    
    [Fact]
    public void Collection_containing_non_nullable_items_with_null_fails()
    {
        SetPropertyValue(
            model: _model,
            propertyName: nameof(ParentModel.StringList),
            propertyValue: new List<string?> {"", null});
        var parentModelValidator = _fixture.CreateParentModelValidator(_ => { });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrow<ModelFormatException>();
    }
    
    [Fact]
    public void Non_nullable_nested_model_with_null_fails()
    {
        SetPropertyValue(
            model: _model,
            propertyName: nameof(ParentModel.NestedModel),
            propertyValue: null);
        var parentModelValidator = _fixture.CreateParentModelValidator(_ => { });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrow<ModelFormatException>();
    }

    private void SetPropertyValue(object model, string propertyName, object? propertyValue)
    {
        var property = model
            .GetType()
            .GetProperties()
            .First(x => x.Name == propertyName);
        property.SetValue(_model, propertyValue);
    }
}