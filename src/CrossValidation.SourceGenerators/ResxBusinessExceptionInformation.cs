using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CrossValidation.SourceGenerators;

public readonly struct ResxBusinessExceptionInformation(
    ClassDeclarationSyntax classSyntax,
    ITypeSymbol symbol)
{
    public ClassDeclarationSyntax ClassSyntax => classSyntax;
    public ITypeSymbol Symbol => symbol;
}