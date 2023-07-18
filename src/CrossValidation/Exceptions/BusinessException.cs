using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CrossValidation.Exceptions;

public class BusinessException : Exception
{
    private const string DefaultPlaceholderValue = "";
    private static readonly Regex PlaceholderRegex = new("{([^{}:]+)}", RegexOptions.Compiled);

    public override string Message => FormattedMessage;
    public string? Code { get; set;  } 
    public string FormattedMessage { get; set; } = "";
    public HttpStatusCode StatusCode { get; set; }
    public string? Details { get; set; }
    public string? FieldName { get; set; }
    public string? FieldDisplayName { get; set; }
    public Dictionary<string, object> PlaceholderValues { get; set; } = new();
    public Type? CrossErrorToException { get; set; }
    public Func<object>? GetFieldValue { get; set; }
    public virtual bool IsCommon { get; set; } = false;
    
    public BusinessException(
        string message = "",
        string? code = null,
        HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity,
        string? details = null,
        string? fieldName = null,
        string? fieldDisplayName = null)
    {
        FormattedMessage = GetFormattedMessage(code, message);
        Code = code;
        StatusCode = statusCode;
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

    /// <summary>
    /// Get error constructor parameter names
    /// </summary>
    public IEnumerable<string> GetFieldNames()
    {
        return GetType()
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(x => x.Name!);
    }

    public void AddCommonPlaceholderValues()
    {
        AddPlaceholderValue(FieldName);
        AddPlaceholderValue(FieldDisplayName);

        if (GetFieldValue is not null)
        {
            AddPlaceholderValue(GetFieldValue!(), "FieldValue");
        }
        
        ReplacePlaceholderValues();
        
        if (CrossValidationOptions.LocalizeErrorInClient)
        {
            PlaceholderValues.Clear();
        }
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