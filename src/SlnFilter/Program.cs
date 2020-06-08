using System;
using System.IO;
using System.Linq;
using Microsoft.Ide.ProjectSystem;

namespace SlnFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                PrintHelp();
                return;
            }

            var inputSln = args[0];
            var configTxt = args[1];
            var outputSln = args[2];

            if (!File.Exists(inputSln))
            {
                Console.Error.WriteLine("Input file doesn't exist: " + inputSln);
                return;
            }

            if (!File.Exists(configTxt))
            {
                Console.Error.WriteLine("Config file doesn't exist: " + configTxt);
                return;
            }

            new Program().Process(inputSln, configTxt, outputSln);
        }

        private void Process(string inputSln, string configTxt, string outputSln)
        {
            var solution = SolutionFile.Parse(inputSln);

            var config = IncludeExcludePattern.ParseFromFile(configTxt);

            foreach (var project in solution.ProjectsInOrder.ToArray())
            {
                ProcessProject(solution, project, config);
            }

            var slnText = solution.GetText();

            if (File.Exists(outputSln))
            {
                var existingText = File.ReadAllText(outputSln);
                if (slnText == existingText)
                {
                    return;
                }
            }

            File.WriteAllText(outputSln, slnText);
        }

        private void ProcessProject(SolutionFile solution, ProjectInSolution project, IncludeExcludePattern includeExcludePattern)
        {
            var projectPath = project.ProjectPath;

            if (includeExcludePattern.Excludes(projectPath) || includeExcludePattern.Excludes(project.ProjectName))
            {
                solution.RemoveProject(projectPath);
            }
        }

        private static void PrintHelp()
        {
            var help = @"Usage: slnfilter.exe <input.sln> <config.txt> <output.sln>";
            Console.WriteLine(help);
        }
    }
}
