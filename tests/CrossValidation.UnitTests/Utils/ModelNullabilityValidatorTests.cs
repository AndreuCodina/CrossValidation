﻿using Common.Tests;
using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Fixtures;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Utils;
using Shouldly;
using Xunit;

namespace CrossValidation.UnitTests.Utils;

public class ModelNullabilityValidatorTests :
    TestBase,
    IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public ModelNullabilityValidatorTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void Non_nullable_field_with_null_fails()
    {
        var expectedPropertyName = nameof(ParentModel.String);
        SetPropertyValue(
            model: _model,
            propertyName: expectedPropertyName,
            propertyValue: null);

        var action = () => Validate.ModelNullability(_model);

        var exception = action.ShouldThrow<ModelNullabilityValidatorException.NonNullablePropertyIsNullException>();
        exception.PropertyName.ShouldBe(expectedPropertyName);
    }
    
    [Fact]
    public void Non_nullable_nested_model_with_null_fails()
    {
        var expectedPropertyName = nameof(ParentModel.NestedModel);
        SetPropertyValue(
            model: _model,
            propertyName: expectedPropertyName,
            propertyValue: null);

        var action = () => Validate.ModelNullability(_model);

        var exception = action.ShouldThrow<ModelNullabilityValidatorException.NonNullablePropertyIsNullException>();
        exception.PropertyName.ShouldBe(expectedPropertyName);
    }

    [Fact]
    public void Collection_containing_non_nullable_items_with_null_fails()
    {
        var expectedCollectionName = nameof(ParentModel.StringList);
        SetPropertyValue(
            model: _model,
            propertyName: expectedCollectionName,
            propertyValue: new List<string?> {"", null});

        var action = () => Validate.ModelNullability(_model);

        var exception = action.ShouldThrow<ModelNullabilityValidatorException.NonNullableItemCollectionWithNullItemException>();
        exception.CollectionName.ShouldBe(expectedCollectionName);
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