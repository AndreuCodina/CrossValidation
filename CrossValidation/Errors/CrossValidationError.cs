using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CrossValidation.Errors;

public interface ICrossValidationError
{
    string? FieldName { get; set; }
    string? FieldDisplayName { get; set; }
    object? FieldValue { get; set; }
    string? Code { get; set; }
    string? Message { get; set; }
    string? Details { get; set; }
    Dictionary<string, object>? PlaceholderValues { get; set; }
    void AddPlaceholderValues();
}

public record CrossValidationError : ICrossValidationError
{
    private const string DefaultPlaceholderValue = "";
    private static readonly Regex PlaceholderRegex = new Regex("{([^{}:]+)}", RegexOptions.Compiled);
    
    public string? FieldName { get; set; }
    public string? FieldDisplayName { get; set; }
    public object? FieldValue { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public Dictionary<string, object>? PlaceholderValues { get; set; }

    public CrossValidationError(
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

        if (PlaceholderValues.ContainsKey(name!))
        {
            throw new InvalidOperationException("Cannot add a placeholder with the same name twice");
        }
        
        PlaceholderValues.Add(name!, value);
    }
    
    public virtual void AddPlaceholderValues()
    {
        AddCommonPlaceholderValues();
        AddCustomErrorPlaceholderValues();
        ReplacePlaceholderValues();
    }

    private void AddCommonPlaceholderValues()
    {
        AddPlaceholderValue(FieldDisplayName ?? DefaultPlaceholderValue, nameof(FieldDisplayName));
        AddPlaceholderValue(FieldValue ?? DefaultPlaceholderValue, nameof(FieldValue));
    }

    private void AddCustomErrorPlaceholderValues()
    {
        var arePlaceholderValuesAdded = GetType().GetMethod(nameof(AddPlaceholderValues))!.DeclaringType == GetType();

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
                AddPlaceholderValue(value ?? DefaultPlaceholderValue, name);
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
                return evaluator.Value;

            return value.ToString()!;
        });
    }
}

public record ResXValidationError(string Code) :
    CrossValidationError(Code: Code);