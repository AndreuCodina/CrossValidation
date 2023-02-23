﻿using System.Resources;
using CrossValidation.Resources;

namespace CrossValidation;

public static class CrossValidationOptions
{
    private const string _defaultCultureCode = "en";
    
    /// <summary>
    /// Generates placeholders when they are not added
    /// </summary>
    public static bool LocalizeErrorInClient { get; set; } = SetDefaultLocalizeErrorInClient();
    public static List<ResourceManager> ResourceManagers { get; set; } = SetDefaultResourceManager();
    public static bool HandleUnknownException { get; set; } = SetDefaultHandleUnknownException();
    public static string DefaultCultureCode { get; set; } = SetDefaultCultureCode();
    public static List<string> SupportedCultureCodes { get; set; } = SetDefaultSupportedCultureCodes();

    public static void SetDefaultOptions()
    {
        LocalizeErrorInClient = SetDefaultLocalizeErrorInClient();
        ResourceManagers = SetDefaultResourceManager();
        HandleUnknownException = SetDefaultHandleUnknownException();
        DefaultCultureCode = SetDefaultCultureCode();
        SupportedCultureCodes = SetDefaultSupportedCultureCodes();
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
    
    private static string SetDefaultCultureCode()
    {
        return _defaultCultureCode;
    }
    
    private static List<string> SetDefaultSupportedCultureCodes()
    {
        return new() {_defaultCultureCode};
    }
}