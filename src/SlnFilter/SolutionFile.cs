using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Ide.ProjectSystem
{
    public sealed partial class SolutionFile
    {
        private readonly IEnumerable<string> _headerLines;
        private readonly string _visualStudioVersionLineOpt;
        private readonly string _minimumVisualStudioVersionLineOpt;
        private IEnumerable<ProjectInSolution> _projectBlocks;
        private readonly IEnumerable<SectionBlock> _globalSectionBlocks;
        private readonly Dictionary<Guid, ProjectInSolution> _projectsByGuid;

        public SolutionFile(
            IEnumerable<string> headerLines,
            string visualStudioVersionLineOpt,
            string minimumVisualStudioVersionLineOpt,
            IEnumerable<ProjectInSolution> projectBlocks,
            IEnumerable<SectionBlock> globalSectionBlocks)
        {
            if (headerLines == null)
            {
                throw new ArgumentNullException(nameof(headerLines));
            }

            if (projectBlocks == null)
            {
                throw new ArgumentNullException(nameof(projectBlocks));
            }

            if (globalSectionBlocks == null)
            {
                throw new ArgumentNullException(nameof(globalSectionBlocks));
            }

            _headerLines = headerLines.ToList().AsReadOnly();
            _visualStudioVersionLineOpt = visualStudioVersionLineOpt;
            _minimumVisualStudioVersionLineOpt = minimumVisualStudioVersionLineOpt;
            _projectBlocks = projectBlocks.ToList().AsReadOnly();
            _globalSectionBlocks = globalSectionBlocks.ToList().AsReadOnly();
            _projectsByGuid = new Dictionary<Guid, ProjectInSolution>();

            foreach (var projectBlock in projectBlocks)
            {
                _projectsByGuid[projectBlock.ProjectGuid] = projectBlock;
            }

            foreach (var globalSection in _globalSectionBlocks)
            {
                if (globalSection.ParenthesizedName == "NestedProjects")
                {
                    foreach (var kvp in globalSection.KeyValuePairs)
                    {
                        if (!Guid.TryParse(kvp.Key, out Guid left) || !Guid.TryParse(kvp.Value, out Guid right))
                        {
                            continue;
                        }

                        if (_projectsByGuid.TryGetValue(left, out ProjectInSolution childProject) && _projectsByGuid.TryGetValue(right, out ProjectInSolution parentProject))
                        {
                            childProject.ParentProject = parentProject;
                        }
                    }
                }
            }
        }

        public void RemoveProject(string projectPath)
        {
            var project = ProjectsInOrder.FirstOrDefault(p => IsSameProject(p, projectPath));
            if (project == null)
            {
                return;
            }

            _projectBlocks = _projectBlocks.Where(p => !IsSameProject(p, projectPath)).ToArray();

            var guidString = project.ProjectGuid.ToString("b").ToLowerInvariant();
            ProjectConfigurationPlatformsGlobalSection.RemoveEntry(kvp =>
                kvp.Key.IndexOf(guidString, StringComparison.OrdinalIgnoreCase) != -1);

            NestedProjectsGlobalSection.RemoveEntry(kvp =>
                kvp.Value.IndexOf(guidString, StringComparison.OrdinalIgnoreCase) != -1);

            foreach (var item in ProjectsInOrder)
            {
                var projectDependencies = item.ProjectSections.FirstOrDefault(s => s.ParenthesizedName == "ProjectDependencies");
                if (projectDependencies != null)
                {
                    projectDependencies.RemoveEntry(kvp =>
                        kvp.Key.IndexOf(guidString, StringComparison.OrdinalIgnoreCase) != -1);
                }
            }
        }

        public ProjectInSolution FindProject(string projectPath)
        {
            return ProjectsInOrder.FirstOrDefault(p => IsSameProject(p, projectPath));
        }

        private static bool IsSameProject(ProjectInSolution p, string projectPath)
        {
            return
                string.Equals(p.ProjectPath, projectPath, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.RelativePath, projectPath, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<string> HeaderLines
        {
            get { return _headerLines; }
        }

        public string VisualStudioVersionLineOpt
        {
            get { return _visualStudioVersionLineOpt; }
        }

        public string MinimumVisualStudioVersionLineOpt
        {
            get { return _minimumVisualStudioVersionLineOpt; }
        }

        public IEnumerable<ProjectInSolution> ProjectBlocks
        {
            get { return _projectBlocks; }
        }

        public IEnumerable<ProjectInSolution> ProjectsInOrder => _projectBlocks;
        public IEnumerable<SectionBlock> GlobalSectionBlocks => _globalSectionBlocks;

        public SectionBlock GetGlobalSectionBlock(string name) =>
            GlobalSectionBlocks.FirstOrDefault(s => s.ParenthesizedName == name);

        public SectionBlock SolutionConfigurationPlatformsGlobalSection => GetGlobalSectionBlock("SolutionConfigurationPlatforms");
        public SectionBlock ProjectConfigurationPlatformsGlobalSection => GetGlobalSectionBlock("ProjectConfigurationPlatforms");
        public SectionBlock NestedProjectsGlobalSection => GetGlobalSectionBlock("NestedProjects");
        public SectionBlock SharedMSBuildProjectFiles => GetGlobalSectionBlock("SharedMSBuildProjectFiles");

        public string GetText()
        {
            var builder = new StringBuilder();

            foreach (var headerLine in _headerLines)
            {
                builder.AppendLine(headerLine);
            }

            foreach (var block in _projectBlocks)
            {
                builder.Append(block.GetText());
            }

            builder.AppendLine("Global");

            foreach (var block in _globalSectionBlocks)
            {
                builder.Append(block.GetText(indent: 1));
            }

            builder.AppendLine("EndGlobal");

            return builder.ToString();
        }

        public static SolutionFile Parse(string filePath)
        {
            var text = File.ReadAllText(filePath);
            return Parse(new StringReader(text));
        }

        public static SolutionFile Parse(TextReader reader)
        {
            var headerLines = new List<string>();

            var headerLine1 = reader.ReadLine();
            while (string.IsNullOrWhiteSpace(headerLine1))
            {
                headerLines.Add(headerLine1);
                headerLine1 = reader.ReadLine();
            }

            if (headerLine1 == null || !headerLine1.StartsWith("Microsoft Visual Studio Solution File", StringComparison.Ordinal))
            {
                throw new Exception(string.Format(SolutionFileParserResources.MissingHeaderInSolutionFile, "Microsoft Visual Studio Solution File"));
            }

            headerLines.Add(headerLine1);

            // skip comment lines and empty lines
            while (reader.Peek() != -1 && "#\r\n".Contains((char)reader.Peek()))
            {
                headerLines.Add(reader.ReadLine());
            }

            string visualStudioVersionLineOpt = null;
            if (reader.Peek() == 'V')
            {
                visualStudioVersionLineOpt = GetNextNonEmptyLine(reader);
                if (!visualStudioVersionLineOpt.StartsWith("VisualStudioVersion", StringComparison.Ordinal))
                {
                    throw new Exception(string.Format(SolutionFileParserResources.MissingHeaderInSolutionFile, "VisualStudioVersion"));
                }

                headerLines.Add(visualStudioVersionLineOpt);
            }

            string minimumVisualStudioVersionLineOpt = null;
            if (reader.Peek() == 'M')
            {
                minimumVisualStudioVersionLineOpt = GetNextNonEmptyLine(reader);
                if (!minimumVisualStudioVersionLineOpt.StartsWith("MinimumVisualStudioVersion", StringComparison.Ordinal))
                {
                    throw new Exception(string.Format(SolutionFileParserResources.MissingHeaderInSolutionFile, "MinimumVisualStudioVersion"));
                }

                headerLines.Add(minimumVisualStudioVersionLineOpt);
            }

            var projectBlocks = new List<ProjectInSolution>();

            // Parse project blocks while we have them
            while (reader.Peek() == 'P')
            {
                projectBlocks.Add(ProjectInSolution.Parse(reader));
                while (reader.Peek() != -1 && "#\r\n".Contains((char)reader.Peek()))
                {
                    // Comments and Empty Lines between the Project Blocks are skipped
                    reader.ReadLine();
                }
            }

            // We now have a global block
            var globalSectionBlocks = ParseGlobal(reader);

            // We should now be at the end of the file
            if (reader.Peek() != -1)
            {
                throw new Exception(SolutionFileParserResources.MissingEndOfFileInSolutionFile);
            }

            return new SolutionFile(headerLines, visualStudioVersionLineOpt, minimumVisualStudioVersionLineOpt, projectBlocks, globalSectionBlocks);
        }

        [SuppressMessage("", "RS0001")] // TODO: This suppression should be removed once we have rulesets in place for Roslyn.sln
        private static IEnumerable<SectionBlock> ParseGlobal(TextReader reader)
        {
            if (reader.Peek() == -1)
            {
                return Enumerable.Empty<SectionBlock>();
            }

            if (GetNextNonEmptyLine(reader) != "Global")
            {
                throw new Exception(string.Format(SolutionFileParserResources.MissingLineInSolutionFile, "Global"));
            }

            var globalSectionBlocks = new List<SectionBlock>();

            // The blocks inside here are indented
            while (reader.Peek() != -1 && char.IsWhiteSpace((char)reader.Peek()))
            {
                globalSectionBlocks.Add(SectionBlock.Parse(reader));
            }

            if (GetNextNonEmptyLine(reader) != "EndGlobal")
            {
                throw new Exception(string.Format(SolutionFileParserResources.MissingLineInSolutionFile, "EndGlobal"));
            }

            // Consume potential empty lines at the end of the global block
            while (reader.Peek() != -1 && "\r\n".Contains((char)reader.Peek()))
            {
                reader.ReadLine();
            }

            return globalSectionBlocks;
        }

        private static string GetNextNonEmptyLine(TextReader reader)
        {
            string line = null;

            do
            {
                line = reader.ReadLine();
            }
            while (line != null && line.Trim() == string.Empty);

            return line;
        }
    }
}
