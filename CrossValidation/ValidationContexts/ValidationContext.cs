﻿using CrossValidation.Errors;
using CrossValidation.Resources;

namespace CrossValidation.ValidationContexts;

public class ValidationContext
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public ICrossValidationError? Error { get; set; }
    public List<ICrossValidationError>? ErrorsCollected { get; set; }
    public string FieldName { get; set; } = "";
    public string? FieldDisplayName { get; set; }
    public object? FieldValue { get; set; }
    public bool ExecuteNextValidator { get; set; } = true;
    public string? ParentPath { get; set; }
    public ValidationMode ValidationMode { get; set; } = ValidationMode.StopValidationOnFirstError;
    public bool IsChildContext { get; set; }

    public ValidationContext CloneForChildModelValidator(string parentPath)
    {
        var newContext = new ValidationContext
        {
            IsChildContext = true,
            ParentPath = parentPath,
            ErrorsCollected = ErrorsCollected,
            ValidationMode = ValidationMode
        };
        return newContext;
    }

    public void AddError(ICrossValidationError error)
    {
        AddCustomizationsToError(error);
        ErrorsCollected ??= new List<ICrossValidationError>();
        error.AddPlaceholderValues();
        ErrorsCollected.Add(error);
    }

    public void Clean()
    {
        Code = null;
        Message = null;
        Details = null;
        Error = null;
        ExecuteNextValidator = true;
    }

    private void AddCustomizationsToError(ICrossValidationError error)
    {
        error.FieldName = FieldName;
        error.FieldDisplayName = GetFieldDisplayNameToFill(error);
        error.FieldValue = FieldValue;

        if (Code is not null)
        {
            error.Code = Code;
        }
        
        error.Message = GetMessageToFill(error);

        if (Details is not null)
        {
            error.Details = Details;
        }
    }
    
    private string? GetMessageToFill(ICrossValidationError error)
    {
        if (Message is not null)
        {
            return Message;
        }
        
        if (Code is not null)
        {
            return ErrorResource.ResourceManager.GetString(Code);
        }
        
        if (error.Message is not null)
        {
            return error.Message;
        }

        if (error.Code is not null)
        {
            return ErrorResource.ResourceManager.GetString(error.Code);
        }

        return null;
    }

    private string GetFieldDisplayNameToFill(ICrossValidationError error)
    {
        if (FieldDisplayName is not null)
        {
            return FieldDisplayName;
        }

        return error.FieldName!;
    }
}