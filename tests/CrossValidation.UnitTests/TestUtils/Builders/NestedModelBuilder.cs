using CrossValidation.UnitTests.TestUtils.Models;

namespace CrossValidation.UnitTests.TestUtils.Builders;

public class NestedModelBuilder
{
    public NestedModel Build()
    {
        return new NestedModel
        {
            String = "Genova",
            Int = 1,
            NestedNestedModel = new NestedNestedModel
            {
                Bool = true
            }
        };
    }
}