using System;
using System.IO;
using System.Linq;
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

        foreach (var property in project.AllEvaluatedProperties.OrderBy(p => p.Name))
        {
            Console.WriteLine("{0}={1}", property.Name, property.EvaluatedValue);
        }

        foreach (var itemGroup in project.AllEvaluatedItems.GroupBy(i => i.ItemType).OrderBy(g => g.Key))
        {
            Console.WriteLine("==================\r\n{0}", itemGroup.Key);
            foreach (var item in itemGroup)
            {
                Console.WriteLine("    " + item.EvaluatedInclude);
            }
        }
    }
}