using System;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class EdgeCaseTests : TestBase
{
    private ParentModel _model;

    public EdgeCaseTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Fact]
    public void Transformation_with_null_value_instead_of_use_NotNull_can_fail()
    {
        var action = () => Validate.That(_model.NullableString)
            .Transform(x => x!)
            .Must(x => x.StartsWith("T"));

        action.ShouldThrow<NullReferenceException>();
    }

    [Fact]
    public void Confuse_method_with_validator()
    {
        var action = () => Validate.That(_model.NullableString)
            .When(x => x is not null) // Decides whether to execute the next validator
            .Transform(x => x!) // This method is not a validator and a new validation is created
            .Must(x => x.StartsWith("T")); // This validator will be executed because it hasn't condition

        action.ShouldThrow<NullReferenceException>();
    }
}