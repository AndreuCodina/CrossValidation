namespace CrossValidation.SourceGenerators;

internal class ParentClassInformation
{
    public ParentClassInformation? Child { get; }
    public string Modifiers { get; }
    public string StructureType { get; }
    public string Name { get; }
    public string NameWithRestrictions { get; }
    
    public ParentClassInformation(
        ParentClassInformation? child,
        string modifiers,
        string structureType,
        string name,
        string nameWithRestrictions)
    {
        Child = child;
        Modifiers = modifiers;
        StructureType = structureType;
        NameWithRestrictions = nameWithRestrictions;
        Name = name;
    }
}