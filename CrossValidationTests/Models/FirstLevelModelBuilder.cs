namespace CrossValidationTests.Models;

public class FirstLevelModelBuilder
{
    public FirstLevelModel Build()
    {
        return new FirstLevelModel
        {
            Property = 1,
            SecondLevel = new FirstLevelModel.SecondLevelModel
            {
                Property = 1,
                ThirdLevel = new FirstLevelModel.SecondLevelModel.ThirdLevelModel
                {
                    Property = 1
                }
            }
        };
    }
}