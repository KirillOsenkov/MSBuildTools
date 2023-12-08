using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public class Generate : Task
{
    [Required]
    public ITaskItem[] InputFiles { get; set; }

    [Required]
    public ITaskItem[] OutputFiles { get; set; }

    [Output]
    public string[] OutputItems { get; set; }

    [Output]
    public string OutputProperty { get; set; }

    public override bool Execute()
    {
        var inputFile = InputFiles[0].GetMetadata("FullPath");
        var text = File.ReadAllText(inputFile);

        var outputFile = OutputFiles[0].GetMetadata("FullPath");
        File.WriteAllText(outputFile, text);

        var outputItems = new List<string>();
        outputItems.Add($"Dependency loaded from: {typeof(Dependency).Assembly.Location}");
        outputItems.Add($"Task loaded from: {typeof(Generate).Assembly.Location}");
        outputItems.Add($"Process: {Process.GetCurrentProcess().MainModule.FileName}");
        outputItems.Add($"Command line: {Environment.CommandLine}");

        OutputItems = outputItems.ToArray();

        OutputProperty = $"Input text: {text} Time: {Dependency.UseDependency()}";

        return !Log.HasLoggedErrors;
    }
}