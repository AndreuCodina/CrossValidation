namespace CrossValidationTests.Models;

public class FirstLevelModel
{
    public int Property { get; set; }
    public SecondLevelModel SecondLevel { get; set; }

    public class SecondLevelModel
    {
        public int Property { get; set; }
        public ThirdLevelModel ThirdLevel { get; set; }

        public class ThirdLevelModel
        {
            public int Property { get; set; }
        }
    }
}