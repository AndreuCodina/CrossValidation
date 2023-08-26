using Common.Tests;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validations.ValidationExtensions;

public class ExclusiveRangeTests : TestBase
{
    [Theory]
    [MemberData(nameof(DataInRange))]
    public void Validate_value_is_not_out_of_range<TField>(TField value, TField from, TField to)
        where TField : IComparable<TField>
    {
        AssertIsComparable(value);
        var action = () => Validate.Field(value)
            .ExclusiveRange(from, to);
        action.Should()
            .NotThrow();
    }

    [Theory]
    [MemberData(nameof(DataOutOfRange))]
    public void Fail_when_value_is_out_of_range<TField>(TField value, TField from, TField to)
        where TField : IComparable<TField>
    {
        AssertIsComparable(value);
        var action = () => Validate.Field(value)
            .ExclusiveRange(from, to);
        var exception = action.Should()
            .Throw<CommonException.ExclusiveRangeException<TField>>()
            .Which;
        exception.Code
            .Should()
            .Be(nameof(ErrorResource.ExclusiveRange));
        exception.FromValue
            .Should()
            .Be(from);
        exception.ToValue
            .Should()
            .Be(to);
    }

    public static IEnumerable<object[]> DataInRange()
    {
        var now = DateTime.UtcNow;
        return new List<object[]>
        {
            new object[] { 4, 3, 5 },
            new object[] { now, now.AddDays(-1), now.AddDays(1) },
            new object[] { "b", "a", "c" }
        };
    }

    public static IEnumerable<object[]> DataOutOfRange()
    {
        var now = DateTime.UtcNow;
        return new List<object[]>
        {
            new object[] { 4, 3, 3 },
            new object[] { 4, 5, 5 },
            new object[] { 4, 4, 4 },
            new object[] { now, now, now },
            new object[] { "a", "a", "a" }
        };
    }

    private void AssertIsComparable<TField>(TField value)
    {
        value.Should()
            .BeAssignableTo<IComparable<TField>>();
    }
}