using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        const int count = 1000;

        var rootDirectory = @"C:\temp\LargeSolution";
        if (Directory.Exists(rootDirectory))
        {
            Directory.Delete(rootDirectory, recursive: true);
        }

        Directory.CreateDirectory(rootDirectory);

        string sharedFile = Path.Combine(rootDirectory, "SharedFile.cs");
        File.WriteAllText(sharedFile, 
            $"""
            using System;
            public class Version { "{ }" }
            {"#"}if Foo
            {"#"}endif
            """);

        var sln = Path.Combine(rootDirectory, "LargeSolution.sln");

        var slnText = GenerateSlnText(count);

        File.WriteAllText(sln, slnText);

        for (int i = 0; i < count; i++)
        {
            string projectName = $"Project{i}";
            var projectDirectory = Path.Combine(rootDirectory, projectName);
            Directory.CreateDirectory(projectDirectory);

            var projectFile = Path.Combine(projectDirectory, projectName + ".csproj");
            string refs = "";
            if (i > 0)
            {
                refs =
                    $"""
                    <ProjectReference Include="..\Project{i - 1}.csproj" />

                    """;
            }
            File.WriteAllText(projectFile,
                $"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net472</TargetFramework>
                    <DefineConstants>{projectName}</DefineConstants>
                  </PropertyGroup>
                  <ItemGroup>
                    {refs}<Compile Include="{sharedFile}" />
                  </ItemGroup>
                </Project>
                """);
        }
    }

    private static string GenerateSlnText(int count)
    {
        var sb = new StringBuilder();
        var configs = new StringBuilder();

        for (int i = 0; i < count; i++)
        {
            string guid = "{00000000-0000-0000-0000-" + i.ToString().PadLeft(12, '0') + "}";
            sb.AppendLine(
                $"""
                Project("{guid}") = "Project{i}", "Project{i}\Project{i}.csproj", "{guid}"
                EndProject
                """);

            configs.AppendLine(
                $"""
                {guid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {guid}.Debug|Any CPU.Build.0 = Debug|Any CPU
                """);
        }

        var text =
            $"""
                        
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            {sb}
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		Release|Any CPU = Release|Any CPU
            	EndGlobalSection
            	GlobalSection(SolutionProperties) = preSolution
            		HideSolutionNode = FALSE
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
                {configs}
            	EndGlobalSection
            EndGlobal
            """;

        return text;
    }
}
