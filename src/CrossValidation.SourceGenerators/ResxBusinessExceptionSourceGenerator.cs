using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CrossValidation.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public class ResxBusinessExceptionSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var businessExceptionSyntaxes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsResxBusinessException(syntaxNode),
                transform: GetResxBusinessExceptionOrNull)
            .Where(static syntax => syntax is not null)
            .Select(static (syntax, _) => syntax!.Value)
            .Collect();
        context.RegisterSourceOutput(businessExceptionSyntaxes, GenerateCode);
    }

    private static ExtractedResxBusinessExceptionSyntax? GetResxBusinessExceptionOrNull(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;
        ITypeSymbol? symbol = context.SemanticModel.GetDeclaredSymbol(classSyntax);
        
        if (symbol is null)
        {
            return null;
        }

        var isPartialClassDefinedSeveralTimes = symbol.DeclaringSyntaxReferences.Length > 1;
        
        if (isPartialClassDefinedSeveralTimes)
        {
            return null;
        }

        var attributeContainingTypeSymbol = symbol.ContainingNamespace;

        if (attributeContainingTypeSymbol is null) return null;
        
        cancellationToken.ThrowIfCancellationRequested();
        return new ExtractedResxBusinessExceptionSyntax(classSyntax, symbol);
    }

    private static bool IsResxBusinessException(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classSyntax)
        {
            return false;
        }

        var isPartial = classSyntax.Modifiers
            .Any(x => x.IsKind(SyntaxKind.PartialKeyword));
        
        if (!isPartial)
        {
            return false;
        }

        var baseList = classSyntax.BaseList;

        if (baseList is null)
        {
            return false;
        }
        var baseIdentifierNameSyntax = (IdentifierNameSyntax)baseList.Types[0].Type;
        var baseClassName = baseIdentifierNameSyntax.Identifier.Text;
        return baseClassName == "ResxBusinessException";
    }
    
    private static void GenerateCode(
        SourceProductionContext context,
        ImmutableArray<ExtractedResxBusinessExceptionSyntax> enumerations)
    {
        if (enumerations.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var extractedResxBusinessExceptionSyntax in enumerations)
        {
            var typeNamespace = extractedResxBusinessExceptionSyntax.Symbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{extractedResxBusinessExceptionSyntax.Symbol.ContainingNamespace}.";
            var parentClass = GetParentClass(extractedResxBusinessExceptionSyntax.ClassSyntax);
            var classPath = GetClassPath(parentClass);
            var code = CreateCode(extractedResxBusinessExceptionSyntax, parentClass);

            context.AddSource($"{typeNamespace}{classPath}.{extractedResxBusinessExceptionSyntax.Symbol.Name}.generated.cs", code);
        }
    }

    private static string GetClassPath(ParentClassInformation? parentClass)
    {
        var classPathStringBuilder = new StringBuilder();
        var currentParentClass = parentClass;
        var isFirstClassVisited = true;

        while (currentParentClass is not null)
        {
            if (isFirstClassVisited)
            {
                isFirstClassVisited = false;
            }
            else
            {
                classPathStringBuilder.Append(".");
            }

            classPathStringBuilder.Append(currentParentClass.Name);
            currentParentClass = currentParentClass.Child;
        }
        
        return classPathStringBuilder.ToString();
    }

    private static string CreateCode(
        ExtractedResxBusinessExceptionSyntax extractedResxBusinessExceptionSyntax,
        ParentClassInformation? parentClass)
    {
        var @namespace = extractedResxBusinessExceptionSyntax.Symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : extractedResxBusinessExceptionSyntax.Symbol.ContainingNamespace.ToString();
        var modifiers = extractedResxBusinessExceptionSyntax.ClassSyntax.Modifiers.ToString();
        var classNameWithRestrictions =
            extractedResxBusinessExceptionSyntax.ClassSyntax.Identifier.ToString()
            + extractedResxBusinessExceptionSyntax.ClassSyntax.TypeParameterList
            + " "
            + extractedResxBusinessExceptionSyntax.ClassSyntax.ConstraintClauses.ToString();
        var numberOfParentClasses = 0;
        var constructorParameterList = extractedResxBusinessExceptionSyntax
            .ClassSyntax
            .ChildNodes()
            .FirstOrDefault(x => x.IsKind(SyntaxKind.ParameterList));
        var constructorParameterNames = constructorParameterList?.ChildNodes()
            .Cast<ParameterSyntax>()
            .Select(x => x.Identifier)
            .Select(x => x.Text)
            .ToArray();
        var result =
            $$"""
              // <auto-generated />
              #nullable enable
              {{GenerateNamespaceStart(@namespace)}}
              {{GenerateParentClassStart(parentClass, ref numberOfParentClasses)}}
                  {{modifiers}} class {{classNameWithRestrictions}}
                  {
                      public override string Message => FormattedMessage();
                      public override string FormattedMessage()
                      {
                          {{GenerateFormattedMessageBody(constructorParameterNames)}}
                      }
                  }
              {{GenerateParentClassEnd(numberOfParentClasses)}}
              {{GenerateNamespaceEnd(@namespace)}}
              #nullable restore
              """;
        return result;
    }

    private static string GenerateFormattedMessageBody(string[]? constructorParameterNames)
    {
         if (constructorParameterNames is null || !constructorParameterNames.Any())
         {
             return
                 """
                 return "";
                 """;
         }
         
         var constructorParameterNamesAsString = string.Join(", ", constructorParameterNames);
         return
             $$"""
               if (MessageTemplate is null)
               {
                   return "";
               }
               
               return global::System.String.Format(MessageTemplate, {{constructorParameterNamesAsString}});
               """;
    }

    private static string GenerateParentClassEnd(int numberOfParentClasses)
    {
        var generatedCodeBuilder = new StringBuilder();
        
        for (var i = 0; i < numberOfParentClasses; i++)
        {
            if (numberOfParentClasses > 0)
            {
                generatedCodeBuilder.AppendLine();
            }
            
            generatedCodeBuilder.Append("}");
        }

        return generatedCodeBuilder.ToString();
    }

    private static string GenerateParentClassStart(
        ParentClassInformation? parentClass,
        ref int numberOfParentClasses)
    {
        var generatedCodeBuilder = new StringBuilder();
        var parentClassIterated = parentClass;
        
        while (parentClassIterated is not null)
        {
            if (numberOfParentClasses > 0)
            {
                generatedCodeBuilder.AppendLine();
            }
            
            generatedCodeBuilder.Append(
                $$"""
                  {{parentClassIterated.Modifiers}} {{parentClassIterated.StructureType}} {{parentClassIterated.NameWithRestrictions}}
                  {
                  """);
            numberOfParentClasses++;
            parentClassIterated = parentClassIterated.Child;
        }

        return generatedCodeBuilder.ToString();
    }

    private static string? GenerateNamespaceStart(string? @namespace)
    {
        if (@namespace is null)
        {
            return null;
        }

        return
            $$"""
              namespace {{@namespace}}
              {
              """;
    }
    
    private static string? GenerateNamespaceEnd(string? @namespace)
    {
        if (@namespace is null)
        {
            return null;
        }

        return "}";
    }
    
    static ParentClassInformation? GetParentClass(BaseTypeDeclarationSyntax typeSyntax)
    {
        TypeDeclarationSyntax? parentSyntax = typeSyntax.Parent as TypeDeclarationSyntax;
        ParentClassInformation? parentClassInfo = null;

        while (parentSyntax != null
               && (parentSyntax.IsKind(SyntaxKind.ClassDeclaration)
               || parentSyntax.IsKind(SyntaxKind.StructDeclaration)
               || parentSyntax.IsKind(SyntaxKind.RecordDeclaration)))
        {
            parentClassInfo = new ParentClassInformation(
                child: parentClassInfo,
                modifiers: parentSyntax.Modifiers.ToString(),
                structureType: parentSyntax.Keyword.ValueText,
                name: parentSyntax.Identifier.ToString(),
                nameWithRestrictions:
                    parentSyntax.Identifier.ToString()
                    + parentSyntax.TypeParameterList
                    + " "
                    + parentSyntax.ConstraintClauses.ToString());

            parentSyntax = parentSyntax.Parent as TypeDeclarationSyntax;
        }

        return parentClassInfo;
    }
}