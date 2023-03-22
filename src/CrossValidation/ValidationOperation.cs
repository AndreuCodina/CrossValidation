using System.Diagnostics;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.Validations;
using CrossValidation.Validators;

namespace CrossValidation;

// public interface IValidationOperation
// {
//     public bool IsSync { get; set; }
//     public string? Message { get; set; }
//     bool RunAsync();
//     
//     // public object? FieldValue { get; set; }
//     // public Type FieldValueType { get; set; }
// }

public interface IValidationOperation
{
    public Func<object?>? GetFieldValue { get; set; }
    public Func<IValidator<ICrossError>>? Validator { get; set; }
    public Func<Task<IValidator<ICrossError>>>? AsyncValidator { get; set; }
    // public Action<IValidValidation<TField>>? ValidationScope { get; set; }
    public Func<bool>? ValidationScope { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public ICrossError? Error { get; set; }
    public string? FieldDisplayName { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    // public Type CrossErrorToException { get; set; }
    public bool ExecuteNextValidator { get; set; }
    // void SetValidationScope(Action setScope);
    bool Execute(ValidationContext context);
    void HandleError(ICrossError error, ValidationContext context);
    void TakeCustomizationsFromInstanceError(ICrossError error, ValidationContext context);
    void TakeCustomizationsFromError(ICrossError error);
}

/// <summary>
/// A node of a validation, defined by its validator and customizations.
///
/// In a validation such as:
/// 
/// Validate.That(request.Age)
///     .WithMessage("The age is required").NotNull() // ValidationNode
///     .WithMessage("The age must be greater than or equal to 18").GreaterThanOrEqual(18) // ValidationNode
///     .Transform(age => age + 1) // ValidationNode
/// </summary>
// public class ValidationOperation<TField> : IValidationOperation
public class ValidationOperation<TField> : IValidationOperation
{
    public Func<object?>? GetFieldValue { get; set; }
    public Func<IValidator<ICrossError>>? Validator { get; set; }
    public Func<Task<IValidator<ICrossError>>>? AsyncValidator { get; set; }
    // public Action<IValidValidation<TField>>? ValidationScope { get; set; }
    public Func<bool>? ValidationScope { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public ICrossError? Error { get; set; }
    public string? FieldDisplayName { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    // public Type CrossErrorToException { get; set; }
    public bool ExecuteNextValidator { get; set; } = true;

    // public void SetValidationScope(Action setScope);
    // {
    //     ValidationScope = validationScope;
    // }

    public bool Execute(ValidationContext context)
    {
        if (Validator is not null)
        {
            var error = Validator!().GetError();

            if (error is null)
            {
                return true;
            }

            // TODO
            // error.CrossErrorToException = CrossErrorToException;
            HandleError(error, context);
            // validValidation.Clean();
            return false;
        }
        else if (ValidationScope is not null)
        {
            return ValidationScope();
        }
        else
        {
            throw new UnreachableException();
        }
    }
    
    public void HandleError(ICrossError error, ValidationContext context)
    {
        var errorToAdd = Error ?? error;
        AddError(errorToAdd, context);

        if (context is {ValidationMode: ValidationMode.StopValidationOnFirstError})
        {
            if (context.ErrorsCollected!.Count == 1)
            {
                throw context.ErrorsCollected[0].ToException();
            }
        }
    }
    
    private void AddError(ICrossError error, ValidationContext context)
    {
        AddCustomizationsToError(error, context);
        context.ErrorsCollected ??= new List<ICrossError>();
        error.AddPlaceholderValues();
        context.ErrorsCollected.Add(error);
    }

    private void AddCustomizationsToError(ICrossError error, ValidationContext context)
    {
        error.FieldName = context.FieldName;
        error.FieldDisplayName = GetFieldDisplayNameToFill(error);
        error.Code = GetCodeToFill(error, context);
        error.Message = GetMessageToFill(error, context);
        error.Details = Details ?? error.Details;
        error.HttpStatusCode = HttpStatusCode ?? error.HttpStatusCode;
    }

    private string? GetCodeToFill(ICrossError error, ValidationContext context)
    {
        if (Code is not null)
        {
            return Code;
        }
        
        if (context.GeneralizeError)
        {
            return nameof(ErrorResource.General);
        }

        return error.Code;
    }

    private string? GetMessageToFill(ICrossError error, ValidationContext context)
    {
        if (Message is not null)
        {
            return Message;
        }

        if (Code is not null)
        {
            return CrossValidationOptions.GetMessageFromCode(Code);
        }
        
        if (context.GeneralizeError)
        {
            return CrossValidationOptions.GetMessageFromCode(nameof(ErrorResource.General));
        }

        if (error.Message is not null)
        {
            return error.Message;
        }

        if (error.Code is not null)
        {
            return CrossValidationOptions.GetMessageFromCode(error.Code);
        }

        return null;
    }

    private string? GetFieldDisplayNameToFill(ICrossError error)
    {
        if (FieldDisplayName is not null)
        {
            return FieldDisplayName;
        }

        if (error.FieldDisplayName is not null)
        {
            return error.FieldDisplayName;
        }

        if (error.FieldName is not null)
        {
            return error.FieldName;
        }

        return null;
    }
    
    public void TakeCustomizationsFromInstanceError(ICrossError error, ValidationContext context)
    {
        if (!context.GeneralizeError)
        {
            return;
        }

        var codeToAdd = Code ?? error.Code;
        var isInstanceCallerWithCodeAndWithoutMessage = Code is not null && Message is null;
        
        if (isInstanceCallerWithCodeAndWithoutMessage && codeToAdd is not null)
        {
            Message = CrossValidationOptions.GetMessageFromCode(codeToAdd);
        }
        else
        {
            Message ??= error.Message;
        }

        Code = codeToAdd;
    }
    
    public void TakeCustomizationsFromError(ICrossError error)
    {
        Code = error.Code ?? Code;
        Message = error.Message ?? Message;
        Details = error.Details ?? Details;
        HttpStatusCode = error.HttpStatusCode ?? HttpStatusCode;
        FieldDisplayName = error.FieldDisplayName ?? FieldDisplayName;
    }
}