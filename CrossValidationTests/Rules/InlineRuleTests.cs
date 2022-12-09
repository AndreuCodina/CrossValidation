using CrossValidationTests.Models;

namespace CrossValidationTests.Rules;

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