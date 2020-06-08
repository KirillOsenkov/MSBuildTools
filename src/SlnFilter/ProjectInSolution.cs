using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Ide.ProjectSystem
{
    public sealed partial class ProjectInSolution
    {
        public Guid ProjectTypeGuid { get; }
        public string ProjectTypeGuidText { get; }
        public string ProjectName { get; }
        public string ProjectPath { get; }
        public Guid ProjectGuid { get; }
        public string ProjectGuidText { get; }
        public IEnumerable<SectionBlock> ProjectSections { get; }

        private static readonly Guid vbProjectGuid = Guid.Parse("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}");
        private static readonly Guid csProjectGuid = Guid.Parse("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
        private static readonly Guid vjProjectGuid = Guid.Parse("{E6FDF86B-F3D1-11D4-8576-0002A516ECE8}");
        private static readonly Guid vcProjectGuid = Guid.Parse("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}");
        private static readonly Guid fsProjectGuid = Guid.Parse("{F2A71F9B-5D33-465A-A702-920D77279786}");
        private static readonly Guid dbProjectGuid = Guid.Parse("{C8D11400-126E-41CD-887F-60BD40844F9E}");
        private static readonly Guid wdProjectGuid = Guid.Parse("{2CFEAB61-6A3B-4EB8-B523-560B4BEEF521}");
        private static readonly Guid webProjectGuid = Guid.Parse("{E24C65DC-7377-472B-9ABA-BC803B73C61A}");
        private static readonly Guid solutionFolderGuid = Guid.Parse("{2150E333-8FDC-42A3-9474-1A3956D46DE8}");

        /// <param name="projectTypeGuidText">To preserve the original case in the .sln (can be lower or upper)</param>
        public ProjectInSolution(
            Guid projectTypeGuid,
            string projectTypeGuidText,
            string projectName,
            string projectPath,
            Guid projectGuid,
            string projectGuidText,
            IEnumerable<SectionBlock> projectSections)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentException(string.Format(SolutionFileParserResources.StringIsNullOrEmpty, "projectName"));
            }

            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentException(string.Format(SolutionFileParserResources.StringIsNullOrEmpty, "projectPath"));
            }

            ProjectTypeGuid = projectTypeGuid;
            ProjectTypeGuidText = projectTypeGuidText;
            ProjectName = projectName;
            ProjectPath = projectPath;
            ProjectGuid = projectGuid;
            ProjectGuidText = projectGuidText;
            ProjectSections = projectSections.ToList().AsReadOnly();
        }

        public string RelativePath => ProjectPath;

        public ProjectInSolution ParentProject { get; internal set; }
        public Guid ParentProjectGuid => ParentProject?.ProjectGuid ?? Guid.Empty;

        internal string GetText()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", ProjectTypeGuidText, ProjectName, ProjectPath, ProjectGuidText);
            builder.AppendLine();

            foreach (var block in ProjectSections)
            {
                builder.Append(block.GetText(indent: 1));
            }

            builder.AppendLine("EndProject");

            return builder.ToString();
        }

        internal static ProjectInSolution Parse(TextReader reader)
        {
            var startLine = reader.ReadLine().TrimStart(null);
            var scanner = new LineScanner(startLine);

            if (scanner.ReadUpToAndEat("(\"") != "Project")
            {
                throw new Exception(string.Format(SolutionFileParserResources.InvalidProjectBlockInSolutionFile4, "Project"));
            }

            var projectTypeGuidText = scanner.ReadUpToAndEat("\")");
            var projectTypeGuid = Guid.Parse(projectTypeGuidText);

            // Read chars upto next quote, must contain "=" with optional leading/trailing whitespaces.
            if (scanner.ReadUpToAndEat("\"").Trim() != "=")
            {
                throw new Exception(SolutionFileParserResources.InvalidProjectBlockInSolutionFile);
            }

            var projectName = scanner.ReadUpToAndEat("\"");

            // Read chars upto next quote, must contain "," with optional leading/trailing whitespaces.
            if (scanner.ReadUpToAndEat("\"").Trim() != ",")
            {
                throw new Exception(SolutionFileParserResources.InvalidProjectBlockInSolutionFile2);
            }

            var projectPath = scanner.ReadUpToAndEat("\"");

            // Read chars upto next quote, must contain "," with optional leading/trailing whitespaces.
            if (scanner.ReadUpToAndEat("\"").Trim() != ",")
            {
                throw new Exception(SolutionFileParserResources.InvalidProjectBlockInSolutionFile3);
            }

            var projectGuidText = scanner.ReadUpToAndEat("\"");
            Guid.TryParse(projectGuidText, out var projectGuid);

            var projectSections = new List<SectionBlock>();

            while (((char)reader.Peek()) is char c && (char.IsWhiteSpace(c) || c == 'P'))
            {
                projectSections.Add(SectionBlock.Parse(reader));
            }

            // Expect to see "EndProject" but be tolerant with missing tags as in Dev12.
            // Instead, we may see either P' for "Project" or 'G' for "Global", which will be handled next.
            if (reader.Peek() != 'P' && reader.Peek() != 'G')
            {
                if (reader.ReadLine() != "EndProject")
                {
                    throw new Exception(string.Format(SolutionFileParserResources.InvalidProjectBlockInSolutionFile4, "EndProject"));
                }
            }

            return new ProjectInSolution(projectTypeGuid, projectTypeGuidText, projectName, projectPath, projectGuid, projectGuidText, projectSections);
        }
    }
}
