using System.Collections.Frozen;
using System.Resources;
using CrossValidation.ErrorResources;

namespace CrossValidation;

public static class CrossValidationOptions
{
    private const string _defaultCultureCode = "en";
    public const string ErrorCodePagePath = "error-codes";
    
    /// <summary>
    /// Generates placeholders when they are not added
    /// </summary>
    public static bool LocalizeCommonErrorsInFront { get; set; }
    public static FrozenSet<ResourceManager> ResourceManagers { get; set; }
    public static bool CustomizeHttpResponse { get; set; }
    public static string DefaultCultureCode { get; set; }
    public static List<string> SupportedCultureCodes { get; set; }
    public static bool IsErrorCodePageEnabled { get; set; }
    public static string? PublicationUrl { get; set; }

    static CrossValidationOptions()
    {
        LocalizeCommonErrorsInFront = SetDefaultLocalizeCommonErrorsInFront();
        ResourceManagers = SetDefaultResourceManagers();
        CustomizeHttpResponse = SetDefaultCustomizeHttpResponse();
        DefaultCultureCode = SetDefaultCultureCode();
        SupportedCultureCodes = SetDefaultSupportedCultureCodes();
        IsErrorCodePageEnabled = SetDefaultIsErrorCodePageEnabled();
        PublicationUrl = SetDefaultPublicationUrl();
    }
    
    public static void SetDefaultOptions()
    {
        LocalizeCommonErrorsInFront = SetDefaultLocalizeCommonErrorsInFront();
        ResourceManagers = SetDefaultResourceManagers();
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
        ResourceManagers = ResourceManagers.ToList()
            .Prepend(resourceManager)
            .ToFrozenSet();
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
    
    private static FrozenSet<ResourceManager> SetDefaultResourceManagers()
    {
        var resourceType = typeof(ErrorResource);
        var resourceBaseName = resourceType.FullName!;
        var resourceManager = new ResourceManager(resourceBaseName, resourceType.Assembly);
        return new List<ResourceManager> { resourceManager }
            .ToFrozenSet();
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
        return new() { _defaultCultureCode };
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