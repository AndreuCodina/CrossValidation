using System.Resources;
using CrossValidation.Resources;

namespace CrossValidation;

public static class CrossValidationOptions
{
    private const string _defaultCultureCode = "en";
    public const string ErrorCodePagePath = "error-codes";
    
    /// <summary>
    /// Generates placeholders when they are not added
    /// </summary>
    public static bool LocalizeCommonErrorsInFront { get; set; } = SetDefaultLocalizeCommonErrorsInFront();
    public static List<ResourceManager> ResourceManagers { get; set; } = SetDefaultResourceManager();
    public static bool CustomizeHttpResponse { get; set; } = SetDefaultCustomizeHttpResponse();
    public static string DefaultCultureCode { get; set; } = SetDefaultCultureCode();
    public static List<string> SupportedCultureCodes { get; set; } = SetDefaultSupportedCultureCodes();
    public static bool IsErrorCodePageEnabled { get; set; } = SetDefaultIsErrorCodePageEnabled();
    public static string? PublicationUrl { get; set; } = SetDefaultPublicationUrl();

    public static void SetDefaultOptions()
    {
        LocalizeCommonErrorsInFront = SetDefaultLocalizeCommonErrorsInFront();
        ResourceManagers = SetDefaultResourceManager();
        CustomizeHttpResponse = SetDefaultCustomizeHttpResponse();
        DefaultCultureCode = SetDefaultCultureCode();
        SupportedCultureCodes = SetDefaultSupportedCultureCodes();
        IsErrorCodePageEnabled = SetDefaultIsErrorCodePageEnabled();
        PublicationUrl = SetDefaultPublicationUrl();
    }

    public static void AddResourceManager<TResourceFile>()
    {
        var resourceType = typeof(TResourceFile);
        var resourceBaseName = resourceType.FullName!;
        var resourceManager = new ResourceManager(resourceBaseName, resourceType.Assembly);
        ResourceManagers = ResourceManagers.Prepend(resourceManager)
            .ToList();
    }
    
    public static string GetMessageFromCode(string code)
    {
        var message = "";

        foreach (var resourceManager in ResourceManagers)
        {
            message = resourceManager.GetString(code) ?? "";
            
            if (message != "")
            {
                break;
            }
        }

        return message;
    }
    
    private static bool SetDefaultLocalizeCommonErrorsInFront()
    {
        return false;
    }
    
    private static List<ResourceManager> SetDefaultResourceManager()
    {
        var resourceType = typeof(ErrorResource);
        var resourceBaseName = resourceType.FullName!;
        var resourceManager = new ResourceManager(resourceBaseName, resourceType.Assembly);
        return new List<ResourceManager> {resourceManager};
    }
    
    private static bool SetDefaultCustomizeHttpResponse()
    {
        return true;
    }
    
    private static string SetDefaultCultureCode()
    {
        return _defaultCultureCode;
    }
    
    private static List<string> SetDefaultSupportedCultureCodes()
    {
        return new() {_defaultCultureCode};
    }
    
    private static bool SetDefaultIsErrorCodePageEnabled()
    {
        return false;
    }
    
    private static string? SetDefaultPublicationUrl()
    {
        return null;
    }
}