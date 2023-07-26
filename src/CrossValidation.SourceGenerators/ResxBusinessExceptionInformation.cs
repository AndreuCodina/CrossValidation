using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CrossValidation.SourceGenerators;

public record struct ResxBusinessExceptionInformation(
    ClassDeclarationSyntax ClassSyntax,
    ITypeSymbol Symbol);