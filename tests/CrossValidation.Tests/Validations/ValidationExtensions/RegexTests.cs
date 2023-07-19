using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class RegexTests : TestBase
{
    private ParentModel _model;

    public RegexTests()
    {
        _model = new ParentModelBuilder()
            .Build();
    }
    
    [Fact]
    public void Not_fail_when_field_value_satisfies_pattern()
    {
        var value = "name";
        var pattern = "[a-z]";
        var action = () => Validate.Field(value)
            .Regex(pattern);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Fail_when_field_value_does_not_satisfy_pattern()
    {
        var value = "1";
        var pattern = "[a-z]";
        var action = () => Validate.Field(value)
            .Regex(pattern);

        var exception = action.ShouldThrow<CommonCrossException.RegularExpression>();
        
        exception.Code.ShouldBe(nameof(ErrorResource.RegularExpression));
    }
}