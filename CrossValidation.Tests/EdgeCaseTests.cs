using System;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Models;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class EdgeCaseTests
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
            .Transform(x => x!) // This method is not a validator
            .Must(x => x.StartsWith("T")); // This validator will not be executed

        action.ShouldNotThrow();
    }
}