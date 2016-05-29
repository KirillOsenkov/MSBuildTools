using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace ProjectReferenceGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ProjectReferenceGraph.exe <path-to-sln>.sln");
                return;
            }

            var solutionFilePath = args[0];
            if (!File.Exists(solutionFilePath))
            {
                Console.Error.WriteLine("Solution file not found: " + solutionFilePath);
                return;
            }

            var references = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            var solution = SolutionFile.Parse(solutionFilePath);
            foreach (var projectInSolution in solution.ProjectsInOrder)
            {
                if (projectInSolution.ProjectType != SolutionProjectType.KnownToBeMSBuildFormat)
                {
                    continue;
                }

                var projectFilePath = Path.GetFullPath(projectInSolution.AbsolutePath);
                var project = ProjectCollection.GlobalProjectCollection.LoadProject(projectFilePath);
                var projectReferences = project.GetItems("ProjectReference");
                foreach (var projectReference in projectReferences)
                {
                    var relativePath = projectReference.EvaluatedInclude;
                    var projectDirectory = projectReference.GetMetadataValue("ProjectDirectory");
                    var absolutePath = Path.GetFullPath(Path.Combine(projectDirectory, relativePath));

                    HashSet<string> bucket;
                    if (!references.TryGetValue(projectFilePath, out bucket))
                    {
                        bucket = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        references.Add(projectFilePath, bucket);
                    }

                    bucket.Add(absolutePath);
                }
            }

            foreach (var bucket in references)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(bucket.Key);
                foreach (var reference in bucket.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("    " + reference);
                }
            }

            var outputFilePath = Path.ChangeExtension(solutionFilePath, ".dgml");
            using (var streamWriter = new StreamWriter(outputFilePath, append: false, encoding: Encoding.UTF8))
            {
                streamWriter.WriteLine("<DirectedGraph xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">");
                streamWriter.WriteLine("  <Nodes>");
                foreach (var kvp in references)
                {
                    streamWriter.WriteLine($"    <Node Id=\"{GetNodeName(kvp.Key)}\" />");
                }

                streamWriter.WriteLine("  </Nodes>");
                streamWriter.WriteLine("  <Links>");
                foreach (var kvp in references)
                {
                    foreach (var reference in kvp.Value)
                    {
                        streamWriter.WriteLine($"    <Link Source=\"{GetNodeName(kvp.Key)}\" Target=\"{GetNodeName(reference)}\" />");
                    }
                }

                streamWriter.WriteLine("  </Links>");
                streamWriter.WriteLine("</DirectedGraph>");
            }
        }

        private static string GetNodeName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }
    }
}
