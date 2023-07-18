using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CrossValidation.SourceGenerators;

public readonly struct ExtractedResxBusinessExceptionSyntax(
    ClassDeclarationSyntax classSyntax,
    ITypeSymbol symbol)
{
    public ClassDeclarationSyntax ClassSyntax { get; } = classSyntax;
    public ITypeSymbol Symbol { get; } = symbol;
}