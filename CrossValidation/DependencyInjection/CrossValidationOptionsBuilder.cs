using System.Globalization;

namespace CrossValidation.DependencyInjection;

public class CrossValidationOptionsBuilder
{
    public CrossValidationOptionsBuilder LocalizeErrorInClient()
    {
        CrossValidationOptions.GeneratePlaceholderValuesWhenTheyAreNotAdded = true;
        return this;
    }
    
    public CrossValidationOptionsBuilder AddResx<TResourceFile>()
    {
        CrossValidationOptions.AddResourceManager<TResourceFile>();
        return this;
    }
    
    public CrossValidationOptionsBuilder NotHandleUnknownException()
    {
        CrossValidationOptions.HandleUnknownException = false;
        return this;
    }
    
    public CrossValidationOptionsBuilder SetDefaultCulture(string culture)
    {
        CrossValidationOptions.DefaultCulture = culture;
        return this;
    }
    
    public CrossValidationOptionsBuilder SetSupportedCultures(params string[] cultures)
    {
        CrossValidationOptions.SupportedCultures = cultures.Select(x => new CultureInfo(x))
            .ToList();
        return this;
    }
}