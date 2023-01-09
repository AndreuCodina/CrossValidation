using System.Collections.Generic;
using System.Linq;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Models;
using CrossValidation.Utils;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Utils;

public class FieldInformationExtractorTests
{
    private ParentModel _model;

    public FieldInformationExtractorTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void Select_with_method_call_fails()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {1})
            .Build();

        var methodCall = () => FieldInformationExtractor<int>
            .Extract(_model, x => x.NullableIntList!.First());
        methodCall.ShouldThrowError<FieldInformationExtractorError.CodeCallInFieldSelectorError>();

        var arrayIndexAccess = () => FieldInformationExtractor<int>
            .Extract(_model, x => x.NullableIntList![0]);
        arrayIndexAccess.ShouldThrowError<FieldInformationExtractorError.CodeCallInFieldSelectorError>();
    }

    [Fact]
    public void Select_null_field_returns_null_value()
    {
        var result = FieldInformationExtractor<int?>
            .Extract(_model, x => x.NullableInt);

        result.Value.ShouldBeNull();
    }

    [Fact]
    public void Select_same_model_returns_model()
    {
        var result = FieldInformationExtractor<ParentModel>
            .Extract(_model, x => x);

        result.Value.ShouldBe(_model);
    }

    [Fact]
    public void Select_child_model_returns_child_model()
    {
        var result = FieldInformationExtractor<NestedModel>
            .Extract(_model, x => x.NestedModel);

        result.Value.ShouldNotBeNull();
    }
}