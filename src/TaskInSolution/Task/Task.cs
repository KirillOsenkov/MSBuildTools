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
    public ITaskItem[] OutputItems { get; set; }

    [Output]
    public string OutputProperty { get; set; }

    public override bool Execute()
    {
        var inputFile = InputFiles[0].GetMetadata("FullPath");
        var text = File.ReadAllText(inputFile);

        var outputFile = OutputFiles[0].GetMetadata("FullPath");
        File.WriteAllText(outputFile, text);

        OutputProperty = $"Input text: {text}";

        return !Log.HasLoggedErrors;
    }
}