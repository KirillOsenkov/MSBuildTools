using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;

public class MultiProjectEval
{
    public static void Run(string[] args)
    {
        if (args.Length > 1)
        {
            Console.Error.WriteLine("Pass a directory to evaluate");
            return;
        }

        string directory = Environment.CurrentDirectory;
        if (args.Length == 1 && args[0] is string dir && Directory.Exists(dir))
        {
            directory = dir;
        }

        var projects = Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories);
        Eval(projects);
    }

    private static void Eval(string[] projects)
    {
        var collection = new ProjectCollection();
        var logger = new BinaryLogger
        {
            Parameters = "msbuild.binlog"
        };
        collection.RegisterLogger(logger);

        foreach (var projectFilePath in projects)
        {
            var project = collection.LoadProject(projectFilePath);
            var targetFrameworks = project.GetProperty("TargetFrameworks")?.EvaluatedValue;
            if (!string.IsNullOrWhiteSpace(targetFrameworks))
            {
                var tfms = targetFrameworks.Split(';').Select(s => s.Trim());
                foreach (var tfm in tfms)
                {
                    project.SetGlobalProperty("TargetFramework", tfm);
                    project.ReevaluateIfNecessary();
                    GetProperties(project);
                }
            }
            else
            {
                GetProperties(project);
            }
        }

        logger.Shutdown();
        collection.UnregisterAllLoggers();
        collection.Dispose();
    }

    private static void GetProperties(Project project)
    {
        var targetFramework = project.GetProperty("TargetFramework").EvaluatedValue;
        var targetPath = project.GetProperty("TargetPath").EvaluatedValue;
        Console.WriteLine($"{targetFramework}: {targetPath}");
    }
}