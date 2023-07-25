namespace CrossValidation.SourceGenerators;

internal class ParentClassInformation(
    ParentClassInformation? child,
    string modifiers,
    string structureType,
    string name,
    string? genericDeclaration,
    string constraints)
{
    public ParentClassInformation? Child => child;
    public string Modifiers => modifiers;
    public string StructureType => structureType;
    public string Name => name;
    public string? GenericDeclaration => genericDeclaration;
    public string Constraints => constraints;
    public string NameWithRestrictions => $"{Name}{GenericDeclaration} {Constraints}";
}