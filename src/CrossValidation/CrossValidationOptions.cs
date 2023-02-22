using System.Globalization;
using System.Resources;
using CrossValidation.Resources;

namespace CrossValidation;

public static class CrossValidationOptions
{
    private const string _defaultCulture = "en";
    
    /// <summary>
    /// Generates placeholders when they are not added
    /// </summary>
    public static bool LocalizeErrorInClient { get; set; } = SetDefaultLocalizeErrorInClient();
    public static List<ResourceManager> ResourceManagers { get; set; } = SetDefaultResourceManager();
    public static bool HandleUnknownException { get; set; } = SetDefaultHandleUnknownException();
    public static string DefaultCulture { get; set; } = SetDefaultCulture();
    public static List<CultureInfo> SupportedCultures { get; set; } = SetDefaultSupportedCultures();

    public static void SetDefaultOptions()
    {
        LocalizeErrorInClient = SetDefaultLocalizeErrorInClient();
        ResourceManagers = SetDefaultResourceManager();
        HandleUnknownException = SetDefaultHandleUnknownException();
        DefaultCulture = SetDefaultCulture();
        SupportedCultures = SetDefaultSupportedCultures();
    }

    public static void AddResourceManager<TResourceFile>()
    {
        var resourceType = typeof(TResourceFile);
        var resourceBaseName = resourceType.FullName!;
        var resourceManager = new ResourceManager(resourceBaseName, resourceType.Assembly);
        ResourceManagers = ResourceManagers.Prepend(resourceManager)
            .ToList();
    }
    
    public static string? GetMessageFromCode(string code)
    {
        string? message = null;

        foreach (var resourceManager in ResourceManagers)
        {
            message = resourceManager.GetString(code);
            
            if (message is not null)
            {
                break;
            }
        }

        return message;
    }
    
    private static bool SetDefaultLocalizeErrorInClient()
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
    
    private static bool SetDefaultHandleUnknownException()
    {
        return true;
    }
    
    private static string SetDefaultCulture()
    {
        return _defaultCulture;
    }
    
    private static List<CultureInfo> SetDefaultSupportedCultures()
    {
        return new() {new CultureInfo(_defaultCulture)};
    }
}