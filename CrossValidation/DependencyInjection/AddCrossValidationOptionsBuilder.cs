namespace CrossValidation.DependencyInjection;

public class AddCrossValidationOptionsBuilder
{
    public AddCrossValidationOptionsBuilder LocalizeErrorInClient()
    {
        CrossValidationOptions.GeneratePlaceholderValuesWhenTheyAreNotAdded = true;
        return this;
    }
    
    public AddCrossValidationOptionsBuilder AddResx<TResourceFile>()
    {
        CrossValidationOptions.AddResourceManager<TResourceFile>();
        return this;
    }
}