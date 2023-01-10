﻿using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CrossValidation.Errors;

public interface IValidationError
{
    string? FieldName { get; set; }
    string? FieldDisplayName { get; set; }
    object? FieldValue { get; set; }
    string? Code { get; set; }
    string? Message { get; set; }
    string? Details { get; set; }
    Dictionary<string, object>? PlaceholderValues { get; set; }
    void AddPlaceholderValues();
    IEnumerable<string> GetFieldNames();
}

public record ValidationError : IValidationError
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

    protected void AddPlaceholderValue(object? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        PlaceholderValues ??= new();

        if (PlaceholderValues.ContainsKey(name!))
        {
            throw new InvalidOperationException("Cannot add a placeholder with the same name twice");
        }

        PlaceholderValues.Add(name!, value ?? DefaultPlaceholderValue);
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

    private void AddCommonPlaceholderValues()
    {
        AddPlaceholderValue(FieldName);
        AddPlaceholderValue(FieldDisplayName);
        AddPlaceholderValue(FieldValue);
    }

    private void AddCustomErrorPlaceholderValues()
    {
        var arePlaceholderValuesAdded = GetType().GetMethod(nameof(AddPlaceholderValues))!.DeclaringType == GetType();

        if (!arePlaceholderValuesAdded && CrossValidationConfiguration.GeneratePlaceholderValuesWhenTheyAreNotAdded)
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
                return evaluator.Value;

            return value.ToString()!;
        });
    }
}

public record ValidationErrorCode(string Code) :
    ValidationError(Code: Code);

public record ValidationErrorMessage(string Message) :
    ValidationError(Message: Message);