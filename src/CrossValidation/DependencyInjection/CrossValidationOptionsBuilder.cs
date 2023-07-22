using System.Globalization;

namespace CrossValidation.DependencyInjection;

public class CrossValidationOptionsBuilder
{
    public CrossValidationOptionsBuilder LocalizeCommonErrorsInFront()
    {
        CrossValidationOptions.LocalizeCommonErrorsInFront = true;
        return this;
    }
    
    public CrossValidationOptionsBuilder AddResx<TResourceFile>()
    {
        CrossValidationOptions.AddResourceManager<TResourceFile>();
        return this;
    }
    
    public CrossValidationOptionsBuilder AddResxAndAssociatedCultures<TResourceFile>()
    {
        AddResx<TResourceFile>();
        var supportedCultureCodes = new HashSet<string>(CrossValidationOptions.SupportedCultureCodes);
        var resourceType = typeof(TResourceFile);
        var resourceBaseName = resourceType.FullName;
        var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

        foreach (var culture in allCultures)
        {
            var cultureCode = culture.Name;

            if (cultureCode == "")
            {
                continue;
            }
            
            var languageCodeResourceExtension = "_" + cultureCode.Replace('-', '_');
            var typeFullName = resourceBaseName + languageCodeResourceExtension;
            var newResourceType = resourceType.Assembly.GetType(typeFullName);
            
            if (newResourceType is not null)
            {
                supportedCultureCodes.Add(cultureCode);
            }
        }
        
        CrossValidationOptions.SupportedCultureCodes = supportedCultureCodes.ToList();
        return this;
    }
    
    public CrossValidationOptionsBuilder NotHandleUnknownException()
    {
        CrossValidationOptions.HandleUnknownException = false;
        return this;
    }
    
    public CrossValidationOptionsBuilder SetDefaultCulture(string culture)
    {
        CrossValidationOptions.DefaultCultureCode = culture;
        return this;
    }
    
    public CrossValidationOptionsBuilder SetSupportedCultures(params string[] cultureCodes)
    {
        CrossValidationOptions.SupportedCultureCodes = cultureCodes.ToList();
        return this;
    }

    public CrossValidationOptionsBuilder SetPublicationUrl(string publicationUrl)
    {
        CrossValidationOptions.PublicationUrl = publicationUrl;
        return this;
    }
}