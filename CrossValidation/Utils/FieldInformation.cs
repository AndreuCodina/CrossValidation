namespace CrossValidation.Utils;

/// <summary>
/// 
/// </summary>
/// <param name="FieldFullPath"></param>
/// <param name="FieldValue"></param>
/// <param name="IsFieldSelectedDifferentThanModel">Example: True: x => x.SomeField; False: x => x</param>
/// <typeparam name="TField"></typeparam>
public record FieldInformation<TField>(
    string FieldFullPath,
    TField FieldValue,
    bool IsFieldSelectedDifferentThanModel);