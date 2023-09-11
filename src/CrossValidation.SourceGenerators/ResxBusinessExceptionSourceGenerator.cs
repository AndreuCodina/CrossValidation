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
        var businessExceptions = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsTargetForGeneration(syntaxNode),
                transform: GetTargetForGeneration)
            .Where(static syntax => syntax is not null)
            .Select(static (syntax, _) => syntax!.Value)
            .Collect();
        context.RegisterSourceOutput(businessExceptions, Execute);
    }

    private static ResxBusinessExceptionInformation? GetTargetForGeneration(
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

        if (attributeContainingTypeSymbol is null)
        {
            return null;
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        return new ResxBusinessExceptionInformation(classSyntax, symbol);
    }

    private static bool IsTargetForGeneration(SyntaxNode syntaxNode)
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
        
        var baseIdentifierNameSyntax = (IdentifierNameSyntax)baseList.Types[0]
            .Type;
        var baseClassName = baseIdentifierNameSyntax.Identifier
            .Text;
        return baseClassName is "ResxBusinessException" or "FrontBusinessException";
    }
    
    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<ResxBusinessExceptionInformation> businessExceptions)
    {
        if (businessExceptions.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var businessException in businessExceptions)
        {
            var typeNamespace = businessException.Symbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{businessException.Symbol.ContainingNamespace}.";
            var parentClass = GetParentClass(businessException.ClassSyntax);
            var classPath = GetClassPath(parentClass);
            var code = CreateCode(businessException, parentClass);
            context.AddSource($"{typeNamespace}{classPath}.{businessException.Symbol.Name}.generated.cs", code);
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
        ResxBusinessExceptionInformation resxBusinessExceptionInformation,
        ParentClassInformation? parentClass)
    {
        var @namespace = resxBusinessExceptionInformation.Symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : resxBusinessExceptionInformation.Symbol.ContainingNamespace.ToString();
        var modifiers = resxBusinessExceptionInformation.ClassSyntax.Modifiers.ToString();
        var numberOfParentClasses = 0;
        var constructorParameterList = resxBusinessExceptionInformation
            .ClassSyntax
            .ChildNodes()
            .FirstOrDefault(x => x.IsKind(SyntaxKind.ParameterList));
        var constructorParameterNames = constructorParameterList?.ChildNodes()
            .Cast<ParameterSyntax>()
            .Select(x => x.Identifier)
            .Select(x => x.Text)
            .ToArray();
        var constructorParameters = constructorParameterList?.ChildNodes()
            .Cast<ParameterSyntax>()
            .ToArray();
        var code = new StringBuilder();
        GenerateDirectivesStart(code);
        GenerateNamespacesStart(code, @namespace);
        GenerateParentClassesStart(code, parentClass, ref numberOfParentClasses);
        GenerateClassStart(code, modifiers, resxBusinessExceptionInformation);
        GenerateProperties(code, constructorParameters);
        GenerateAddParametersAsPlaceholderValuesMethod(code, constructorParameterNames);
        GenerateClassEnd(code);
        GenerateParentClassesEnd(code, numberOfParentClasses);
        GenerateNamespacesEnd(code, @namespace);
        GenerateDirectivesEnd(code);
        return code.ToString();
    }

    private static string GetClassNameWithRestrictions(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return $"{classDeclarationSyntax.Identifier.ToString()}{classDeclarationSyntax.TypeParameterList} {classDeclarationSyntax.ConstraintClauses.ToString()}";
    }

    private static void GenerateAddParametersAsPlaceholderValuesMethod(StringBuilder code, string[]? constructorParameterNames)
    {
        if (constructorParameterNames is null)
        {
            return;
        }
        
        code.AppendLine(
            """
            public override void AddParametersAsPlaceholderValues()
            {
            """);
        
        foreach (var constructorParameterName in constructorParameterNames)
        {
            code.AppendLine(
                $$"""
                  AddPlaceholderValue({{constructorParameterName}});
                  """);
        }

        code.AppendLine("}");
    }

    private static void GenerateClassEnd(StringBuilder code)
    {
        code.AppendLine("}");
    }

    private static void GenerateClassStart(
        StringBuilder code,
        string modifiers,
        ResxBusinessExceptionInformation resxBusinessExceptionInformation)
    {
        var classNameWithRestrictions = GetClassNameWithRestrictions(resxBusinessExceptionInformation.ClassSyntax);
        code.AppendLine(
            $$"""
              {{modifiers}} class {{classNameWithRestrictions}}
              {
              """);
    }

    private static void GenerateDirectivesEnd(StringBuilder code)
    {
        code.AppendLine("#nullable restore");
    }

    private static void GenerateDirectivesStart(StringBuilder code)
    {
        code.AppendLine(
            """
            // <auto-generated />
            #nullable enable
            """);
    }

    private static void GenerateProperties(StringBuilder code, ParameterSyntax[]? constructorParameters)
    {
        if (constructorParameters is null)
        {
            return;
        }
        
        foreach (var parameter in constructorParameters)
        {
            var identifier = parameter.Identifier.ToString();
            code.AppendLine($"public {parameter.Type!.ToString()} {char.ToUpper(identifier[0])}{identifier.Substring(1)} => {identifier};");
        }
    }

    private static void GenerateParentClassesEnd(StringBuilder code, int numberOfParentClasses)
    {

        for (var i = 0; i < numberOfParentClasses; i++)
        {
            if (numberOfParentClasses > 0)
            {
                code.AppendLine();
            }
            
            code.AppendLine("}");
        }
    }

    private static void GenerateParentClassesStart(StringBuilder code, ParentClassInformation? parentClass,
        ref int numberOfParentClasses)
    {
        var parentClassIterated = parentClass;
        
        while (parentClassIterated is not null)
        {
            code.AppendLine(
                $$"""
                  {{parentClassIterated.Modifiers}} {{parentClassIterated.StructureType}} {{parentClassIterated.NameWithRestrictions}}
                  {
                  """);
            numberOfParentClasses++;
            parentClassIterated = parentClassIterated.Child;
        }
    }

    private static void GenerateNamespacesStart(StringBuilder code, string? @namespace)
    {
        if (@namespace is null)
        {
            return;
        }

        code.AppendLine(
            $$"""
              namespace {{@namespace}}
              {
              """);
    }
    
    private static void GenerateNamespacesEnd(StringBuilder code, string? @namespace)
    {
        if (@namespace is null)
        {
            return;
        }

        code.AppendLine("}");
    }
    
    private static ParentClassInformation? GetParentClass(BaseTypeDeclarationSyntax typeSyntax)
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
                genericDeclaration: parentSyntax.TypeParameterList?.ToString(),
                constraints: parentSyntax.ConstraintClauses.ToString());
            parentSyntax = parentSyntax.Parent as TypeDeclarationSyntax;
        }

        return parentClassInfo;
    }
}