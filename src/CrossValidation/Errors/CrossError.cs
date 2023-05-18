using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CrossValidation.Exceptions;

namespace CrossValidation.Errors;

public interface ICrossError
{
    string? FieldName { get; set; }
    string? FieldDisplayName { get; set; }
    string? Code { get; set; }
    string? Message { get; set; }
    string? Details { get; set; }
    HttpStatusCode? HttpStatusCode { get; set; }
    Dictionary<string, object>? PlaceholderValues { get; set; }
    public Type? CrossErrorToException { get; set; }
    Func<object>? GetFieldValue { get; set; }
    void AddPlaceholderValues();
    IEnumerable<string> GetFieldNames();
    Exception ToException();
}

public record CrossError : ICrossError
{
    private const string DefaultPlaceholderValue = "";
    private static readonly Regex PlaceholderRegex = new("{([^{}:]+)}", RegexOptions.Compiled);

    public string? FieldName { get; set; }
    public string? FieldDisplayName { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    public Dictionary<string, object>? PlaceholderValues { get; set; }
    public Type? CrossErrorToException { get; set; }
    public Func<object>? GetFieldValue { get; set; }

    public CrossError(
        string? FieldName = null,
        string? FieldDisplayName = null,
        string? Code = null,
        string? Message = null,
        string? Details = null,
        HttpStatusCode? HttpStatusCode = null,
        Type? CrossErrorToException = null)
    {
        this.FieldName = FieldName;
        this.FieldDisplayName = FieldDisplayName;
        this.Code = Code;
        this.Message = Message;
        
        if (Message is null && Code is not null)
        {
            this.Message = CrossValidationOptions.GetMessageFromCode(Code);
        }
        
        this.Details = Details;
        this.HttpStatusCode = HttpStatusCode;
        this.CrossErrorToException = CrossErrorToException ?? typeof(CrossException);
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

    public virtual void AddPlaceholderValues()
    {
        AddCommonPlaceholderValues();
        AddCustomErrorPlaceholderValues();
        ReplacePlaceholderValues();
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

    public Exception ToException()
    {
        AddPlaceholderValues();

        var canUseCrossValidationCustomizations = CrossErrorToException!
            .GetInterface(nameof(ICrossErrorToException)) is not null;

        if (!canUseCrossValidationCustomizations)
        {
            throw (Exception)Activator.CreateInstance(CrossErrorToException, CreateExceptionMessage())!;
        }
        
        var fromCrossErrorMethod = CrossErrorToException.GetMethod(
            nameof(ICrossErrorToException.FromCrossError),
            new[] {typeof(ICrossError)})!;
        var parameters = new object[] {this};
        var exception = fromCrossErrorMethod.Invoke(null, parameters);
        return (Exception)exception!;
    }

    private string? CreateExceptionMessage()
    {
        string? message = null;
            
        if (FieldName is not null)
        {
            message = $"{FieldName}: ";
        }

        if (Message is not null)
        {
            if (message is not null)
            {
                message += Message;
            }
            else
            {
                message = Message;
            }
        }
            
        return message;
    }

    private void AddCommonPlaceholderValues()
    {
        AddPlaceholderValue(FieldName);
        AddPlaceholderValue(FieldDisplayName);

        if (GetFieldValue is not null)
        {
            AddPlaceholderValue(GetFieldValue!(), "FieldValue");
        }
    }

    private void AddCustomErrorPlaceholderValues()
    {
        var arePlaceholderValuesAdded = GetType().GetMethod(nameof(AddPlaceholderValues))!.DeclaringType == GetType();

        if (!arePlaceholderValuesAdded && CrossValidationOptions.LocalizeErrorInClient)
        {
            var properties = GetType().GetProperties();
            var customPlaceholderNames = GetFieldNames();

            foreach (var customPlaceholderName in customPlaceholderNames)
            {
                var value = properties.Where(x => x.Name == customPlaceholderName)
                    .Select(x => x.GetValue(this)!)
                    .FirstOrDefault();
                AddPlaceholderValue(value ?? DefaultPlaceholderValue, customPlaceholderName);
            }
        }
    }

    private void ReplacePlaceholderValues()
    {
        if (Message is null)
        {
            return;
        }

        Message = PlaceholderRegex.Replace(Message, evaluator =>
        {
            var key = evaluator.Groups[1].Value;

            if (!PlaceholderValues!.TryGetValue(key, out var value))
            {
                return evaluator.Value;
            }

            return value.ToString()!;
        });
    }
}

public record CodeCrossError(string Code) :
    CrossError(Code: Code);

public record MessageCrossError(string Message) :
    CrossError(Message: Message);