using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace Generator
{
    [Generator]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            string text =
                """
                namespace GeneratedNamespace
                {
                    public partial class GeneratedClass
                    {
                        public static void GeneratedMethod()
                        {
                            System.Console.WriteLine("hello world");
                        }
                    }
                }
                """;

            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "MyGeneratedFile.g.cs",
                SourceText.From(text, Encoding.UTF8)));
        }
    }
}
