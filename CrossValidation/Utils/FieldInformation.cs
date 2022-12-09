namespace CrossValidation.Utils;

public class FieldInformation<TField>
{
    public static FieldInformation<TField> Create(
        string fieldFullPath,
        TField fieldValue,
        bool isMember)
    {
        return new FieldInformation<TField>
        {
            FieldFullPath = fieldFullPath,
            FieldValue = fieldValue,
            IsFieldSelectedDifferentThanModel = isMember
        };
    }

    public string FieldFullPath { get; init; }
    public TField FieldValue { get; init; }

    /// <example>True: x => x.SomeField</example>
    /// <example>False: x => x</example>
    public bool IsFieldSelectedDifferentThanModel { get; init; }
}