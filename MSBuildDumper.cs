using System;
using System.IO;
using Microsoft.Build.Evaluation;

class MSBuildDumper
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: MSBuildDumper <path-to-project.csproj>");
            return;
        }

        var projectFilePath = Path.GetFullPath(args[0]);
        var project = new Project(projectFilePath); // add reference to Microsoft.Build.dll to compile

        foreach (var property in project.AllEvaluatedProperties)
        {
            Console.WriteLine(" {0}={1}", property.Name, property.EvaluatedValue);
        }

        foreach (var item in project.AllEvaluatedItems)
        {
            Console.WriteLine(" {0}: {1}", item.ItemType, item.EvaluatedInclude);
        }
    }
}