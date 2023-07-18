using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ParentNamespace;
using ParentNamespace.ChildNamespace.GrandchildNamespace;
using Shouldly;

namespace CrossValidation.SourceGenerators.UnitTests;

public class ResxBusinessExceptionSourceGeneratorTests
{
    [Fact]
    public void GenerateOutput()
    {
        var code =
            """
            namespace ParentNamespace
            {
                namespace ChildNamespace.GrandchildNamespace
                {
                    internal partial class ParentClass<T>(T generic) where T : class
                    {
                        public partial class ChildClass
                        {
                            internal partial class ExceptionWithParameters<T>(int age, string email, T generic)
                                : ResxBusinessException(key: ErrorResource.ResourceKey)
                                where T : struct;
                        }
                    }
                }
            }
            """;
        var expectedGeneratedCode =
            """
            // <auto-generated />
            #nullable enable
            namespace ParentNamespace.ChildNamespace.GrandchildNamespace
            {
                internal partial class ParentClass<T> where T : class
                {
                    public partial class ChildClass
                    {
                        internal partial class ExceptionWithParameters<T> where T : struct
                        {
                            public override void AddParametersAsPlaceholderValues()
                            {
                                AddPlaceholderValue(age);
                                AddPlaceholderValue(email);
                                AddPlaceholderValue(generic);
                            }
                        }
                    }
                }
            }
            #nullable restore
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create(
            assemblyName: nameof(ResxBusinessExceptionSourceGeneratorTests),
            syntaxTrees: new[] { syntaxTree });
        var generator = new ResxBusinessExceptionSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var _, out var _);
        
        var runResult = driver.GetRunResult();
        var generatedCode = runResult.Results[0]
            .GeneratedSources[0]
            .SourceText
            .ToString();
        var equals = string.Compare(generatedCode, expectedGeneratedCode, CultureInfo.CurrentCulture,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0;
        equals.ShouldBeTrue();
    }

    [Fact]
    public void GenerateMessages()
    {
        new WithNoParametersGlobalNamespaceException()
            .Message
            .ShouldBe("Error message");
        
        new WithParametersGlobalNamespaceException(1, "message")
            .Message
            .ShouldBe("Error message with 1 and message");
        
        new WithNoParametersWithDeclaredNamespaceException()
            .Message
            .ShouldBe("Error message");
        
        new WithParametersWithDeclaredNamespaceException(1, "message")
            .Message
            .ShouldBe("Error message with 1 and message");

        new ParentClass<string>.ChildClass.ExceptionWithParameters<int>(1, 2)
            .Message
            .ShouldBe("Error message with 1 and 2");
    }
}