﻿using System.Resources;
using CrossValidation.Resources;

namespace CrossValidation;

public static class CrossValidationOptions
{
    public static bool GeneratePlaceholderValuesWhenTheyAreNotAdded = false;
    public static List<ResourceManager> ResourceManagers { get; private set; } = CreateDefaultResourceManager();
    public static bool HandleUnknownException { get; set; } = false;

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
    
    private static List<ResourceManager> CreateDefaultResourceManager()
    {
        var resourceType = typeof(ErrorResource);
        var resourceBaseName = resourceType.FullName!;
        var resourceManager = new ResourceManager(resourceBaseName, resourceType.Assembly);
        return new List<ResourceManager> {resourceManager};
    }
}