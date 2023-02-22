using CrossValidation.Tests.Models;

namespace CrossValidation.Tests.Builders;

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