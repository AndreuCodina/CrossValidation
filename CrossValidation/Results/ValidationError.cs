using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CrossValidation.Results;

public record ValidationError
{
    private static readonly Regex _placeholderRegex = new Regex("{([^{}:]+)}", RegexOptions.Compiled);
    private static readonly string _defaultPlaceholderValue = "";
    
    public string? FieldName { get; set; }
    public string? FieldDisplayName { get; set; }
    public object? FieldValue { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public Dictionary<string, object>? PlaceholderValues { get; set; }

    public ValidationError(
        string? FieldName = null,
        string? FieldDisplayName = null,
        object? FieldValue = null,
        string? Code = null,
        string? Message = null,
        string? Details = null)
    {
        this.FieldName = FieldName;
        this.FieldDisplayName = FieldDisplayName;
        this.FieldValue = FieldValue;
        this.Code = Code;
        this.Message = Message;
        this.Details = Details;
    }

    protected void AddPlaceholderValue(object value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        PlaceholderValues ??= new();
        PlaceholderValues.Add(name!, value);
    }
    
    public virtual void AddPlaceHolderValues()
    {
        AddCommonPlaceholderValues();
        AddCustomErrorPlaceholderValues();
        ReplacePlaceholderValues();
    }

    private void AddCommonPlaceholderValues()
    {
        AddPlaceholderValue(FieldDisplayName ?? _defaultPlaceholderValue, nameof(FieldDisplayName));
        AddPlaceholderValue(FieldValue ?? _defaultPlaceholderValue, nameof(FieldValue));
    }

    private void AddCustomErrorPlaceholderValues()
    {
        var arePlaceholderValuesAdded = GetType().GetMethod("AddPlaceHolderValues").DeclaringType == GetType();

        if (!arePlaceholderValuesAdded && CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded)
        {
            var properties = GetType().GetProperties();
            var customPlaceholders = GetType().GetConstructors()
                .Single()
                .GetParameters();

            foreach (var customPlaceholder in customPlaceholders)
            {
                var name = customPlaceholder.Name;
                var value = properties.Where(x => x.Name == name)
                    .Select(x => x.GetValue(this)!)
                    .FirstOrDefault();
                AddPlaceholderValue(value ?? _defaultPlaceholderValue, name);
            }
        }
    }

    private void ReplacePlaceholderValues()
    {
        if (Message is null)
        {
            return;
        }
        
        Message = _placeholderRegex.Replace(Message, evaluator =>
        {
            var key = evaluator.Groups[1].Value;

            if (!PlaceholderValues!.TryGetValue(key, out var value))
                return evaluator.Value;

            return value.ToString()!;
        });
    }
}

public record ResXValidationError(string Code)
    : ValidationError(Code: Code);