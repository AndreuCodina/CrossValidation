using CrossValidation;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests;

public class InlineRuleTests
{
    private readonly FirstLevelModel _model;

    public InlineRuleTests()
    {
        _model = new FirstLevelModelBuilder().Build();
    }
    
    // [Fact]
    // public void Foo()
    // {
    //     var rule = new InlineRule<int>(_model.Property);
    //     var fooo =rule.fieldn;
    //     
    //     fooo.ShouldBe(1);
    // }
}