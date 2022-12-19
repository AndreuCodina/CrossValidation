namespace CrossValidation.Utils;

/// <summary>
/// 
/// </summary>
/// <param name="SelectionFullPath"></param>
/// <param name="Value"></param>
/// <param name="IsFieldSelectedDifferentThanModel">Example: True: x => x.SomeField; False: x => x</param>
/// <typeparam name="TField"></typeparam>
public record FieldInformation<TField>(
    string SelectionFullPath,
    TField? Value,
    bool IsFieldSelectedDifferentThanModel);