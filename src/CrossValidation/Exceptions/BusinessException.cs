using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CrossValidation.Exceptions;

public class BusinessException : Exception
{
    private const string DefaultPlaceholderValue = "";
    private static readonly Regex PlaceholderRegex = new(@"\{([^{}]+)\}", RegexOptions.Compiled);

    public override string Message => FormattedMessage;
    public string? Code { get; set;  }
    public string? CodeUrl { get; set; }
    public string FormattedMessage { get; set; } = "";
    public int StatusCode { get; set; }
    public string? Details { get; set; }
    public string? FieldName { get; set; }
    public string? FieldDisplayName { get; set; }
    public Dictionary<string, object> PlaceholderValues { get; set; } = new();
    public Type? CustomExceptionToThrow { get; set; }
    public Func<object>? GetFieldValue { get; set; }
    
    public BusinessException(
        string? code = null,
        string message = "",
        HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity,
        string? details = null,
        int statusCodeInt = (int)HttpStatusCode.UnprocessableEntity,
        string? fieldName = null,
        string? fieldDisplayName = null)
    {
        Code = code;
        FormattedMessage = GetFormattedMessage(code, message);

        if (statusCode is not HttpStatusCode.UnprocessableEntity)
        {
            StatusCode = (int)statusCode;
        }
        else if (statusCodeInt != (int)HttpStatusCode.UnprocessableEntity)
        {
            StatusCode = statusCodeInt;
        }
        else
        {
            StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
        
        Details = details;
        FieldName = fieldName;
        FieldDisplayName = fieldDisplayName;
        AddParametersAsPlaceholderValues();
        ReplacePlaceholderValues();
    }

    private static string GetFormattedMessage(string? code, string message)
    {
        if (code is not null && message == "")
        {
            return CrossValidationOptions.GetMessageFromCode(code) ?? message;
        }

        return message;
    }
    
    protected void AddPlaceholderValue(
        object? value,
        [CallerArgumentExpression(nameof(value))] string name = default!)
    {
        PlaceholderValues ??= new();

        if (PlaceholderValues.ContainsKey(name))
        {
            throw new InvalidOperationException("Cannot add a placeholder with the same name twice");
        }

        PlaceholderValues.Add(name, value ?? DefaultPlaceholderValue);
    }

    public void AddCommonPlaceholderValues()
    {
        AddPlaceholderValue(FieldName, "fieldName");
        AddPlaceholderValue(FieldDisplayName, "fieldDisplayName");

        if (GetFieldValue is not null)
        {
            AddPlaceholderValue(GetFieldValue!(), "fieldValue");
        }
        
        ReplacePlaceholderValues();
    }

    public virtual void AddParametersAsPlaceholderValues()
    {
    }

    private void ReplacePlaceholderValues()
    {
        if (FormattedMessage == "")
        {
            return;
        }
        
        FormattedMessage = PlaceholderRegex.Replace(FormattedMessage, evaluator =>
        {
            var key = evaluator.Groups[1].Value;

            if (!PlaceholderValues.TryGetValue(key, out var value))
            {
                return evaluator.Value;
            }
            
            return value.ToString()!;
        });
    }
}