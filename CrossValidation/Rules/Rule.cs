﻿using CrossValidation.Resources;
using CrossValidation.Results;
using CrossValidation.ValidationContexts;
using CrossValidation.Validators;

namespace CrossValidation.Rules;

/// <summary>
/// Rule to validate a field
/// </summary>
public abstract class Rule<TSelf, TField, TValidationContext>
    where TValidationContext : ValidationContext
{
    public TField? FieldValue { get; set; }
    public TValidationContext Context { get; set; }

    protected Rule(TField? fieldValue, TValidationContext context)
    {
        IsValid = true;
        FieldValue = fieldValue;
        Context = context;
    }

    protected abstract TSelf GetSelf();

    public TSelf SetValidator(Func<Validator> validator)
    {
        if (Context.ExecuteNextValidator && IsValid)
        {
            var error = validator().GetError();

            if (error is not null)
            {
                FinishWithError(error);
                IsValid = false;
            }
        }

        Context.Clean();
        return GetSelf();
    }

    public bool IsValid { get; set; }

    public void FinishWithError(CrossValidationError error)
    {
        FillErrorWithCustomizations(error);
        HandleError(error);
    }

    protected abstract void HandleError(CrossValidationError error);

    public TSelf WithMessage(string message)
    {
        Context.SetMessage(message);
        return GetSelf();
    }

    public TSelf WithCode(string code)
    {
        Context.SetCode(code);
        return GetSelf();
    }
    
    public TSelf WithError(CrossValidationError error)
    {
        Context.SetError(error);
        return GetSelf();
    }
    
    public TSelf WithFieldDisplayName(string fieldDisplayName)
    {
        Context.SetFieldDisplayName(fieldDisplayName);
        return GetSelf();
    }

    private void FillErrorWithCustomizations(CrossValidationError error)
    {
        error.FieldName = Context.FieldName;
        error.FieldValue = Context.FieldValue;
        error.Message = GetMessageToFill(error);
        error.FieldDisplayName = GetFieldDisplayNameToFill(error);
    }

    private string? GetMessageToFill(CrossValidationError error)
    {
        return Context.Message is null && error.Message is null && error.Code is not null
            ? ErrorResource.ResourceManager.GetString(error.Code)
            : Context.Message;
    }
    
    private string GetFieldDisplayNameToFill(CrossValidationError error)
    {
        if (Context.FieldDisplayName is not null)
        {
            return Context.FieldDisplayName;
        }
        else if (error.FieldDisplayName is not null and not "")
        {
            return error.FieldDisplayName;
        }

        return error.FieldName!;
    }
}