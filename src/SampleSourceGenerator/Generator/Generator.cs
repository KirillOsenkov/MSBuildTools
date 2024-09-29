using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Generator
{
    [Generator]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static postInitializationContext =>
                postInitializationContext.AddSource("GeneratedAttribute.cs", SourceText.From("""
                using System;
                namespace GeneratedNamespace
                {
                    [AttributeUsage(AttributeTargets.Method)]
                    internal sealed class GeneratedAttribute : Attribute
                    {
                        public static void GeneratedMethod()
                        {
                            System.Console.WriteLine("hello world");
                        }
                    }
                }
                """, Encoding.UTF8)));

            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "GeneratedNamespace.GeneratedAttribute",
                predicate: static (syntaxNode, cancellationToken) => syntaxNode is BaseMethodDeclarationSyntax,
                transform: static (context, cancellationToken) =>
                {
                    var containingClass = context.TargetSymbol.ContainingType;
                    return new Model(
                        Namespace: containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                        ClassName: containingClass.Name,
                        MethodName: context.TargetSymbol.Name);
                }
            );

            context.RegisterSourceOutput(pipeline, static (context, model) =>
            {
                var sourceText = SourceText.From($$"""
                namespace {{model.Namespace}};
                partial class {{model.ClassName}}
                {
                    partial void {{model.MethodName}}()
                    {
                        // generated code
                    }
                }
                """, Encoding.UTF8);

                context.AddSource($"{model.ClassName}_{model.MethodName}.g.cs", sourceText);
            });
        }

        private record Model(string Namespace, string ClassName, string MethodName);
    }
}

#if (!NET5_0_OR_GREATER)
namespace System.Runtime.CompilerServices
{
    /// <nodoc />
    public sealed class IsExternalInit
    {
    }
}
#endif