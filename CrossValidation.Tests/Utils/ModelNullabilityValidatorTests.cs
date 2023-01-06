using System.Collections.Generic;
using System.Linq;
using CrossValidation.Extensions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using CrossValidation.Utils;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Utils;

public class ModelNullabilityValidatorTests : IClassFixture<CommonFixture>
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
        SetPropertyValue(
            model: _model,
            propertyName: nameof(ParentModel.String),
            propertyValue: null);
        var parentModelValidator = _commonFixture.CreateParentModelValidator(_ => { });

        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrowError<ModelNullabilityValidatorError.NonNullablePropertyIsNull>();
    }

    [Fact]
    public void Collection_containing_non_nullable_items_with_null_fails()
    {
        var expectedCollectionName = nameof(ParentModel.StringList);
        SetPropertyValue(
            model: _model,
            propertyName: expectedCollectionName,
            propertyValue: new List<string?> {"", null});
        var parentModelValidator = _commonFixture.CreateParentModelValidator(_ => { });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowError<ModelNullabilityValidatorError.NonNullableItemCollectionWithNullItem>();
        error.CollectionName.ShouldBe(expectedCollectionName);
    }

    [Fact]
    public void Non_nullable_nested_model_with_null_fails()
    {
        var expectedPropertyName = nameof(ParentModel.NestedModel);
        SetPropertyValue(
            model: _model,
            propertyName: expectedPropertyName,
            propertyValue: null);
        var parentModelValidator = _commonFixture.CreateParentModelValidator(_ => { });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowError<ModelNullabilityValidatorError.NonNullablePropertyIsNull>();
        error.PropertyName.ShouldBe(expectedPropertyName);
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