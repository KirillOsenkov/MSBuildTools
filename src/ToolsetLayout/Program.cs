using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToolsetLayout
{
    public class Entrypoint
    {
        static void Main(string[] args)
        {
            var bootstrapRoot = @"C:\msbuild\artifacts\bin\bootstrap\net472";

            new Bootstrap(bootstrapRoot).Prune();
        }
    }

    public class Bootstrap
    {
        public Bootstrap(string root)
        {
            this.Root = root;
        }

        public string Root { get; }

        public void Prune()
        {
            var itemsToDelete = new[]
            {
                @"MSBuild\Microsoft\VisualStudio\NodeJs",
                @"MSBuild\Current\Bin\amd64",
                @"MSBuild\Current\Bin\*.xml",
                @"MSBuild\Sdks\Microsoft.NET.Sdk.Razor\tools\netcoreapp3.0\*.xml",
                @"MSBuild\Sdks\NuGet.Build.Tasks.Pack\**\*.xml",
                @"Team Tools\Static Analysis Tools\FxCop\Microsoft.VisualStudio.CodeAnalysis.Phoenix.xml",
            };

            foreach (var item in itemsToDelete)
            {
                DeleteItem(item);
            }
        }

        public bool DeleteWildcard(string path)
        {
            var parts = path.Split('\\').ToList();

            var root = Root;
            while (parts.Count > 0 && !ContainsWildcard(parts[0]))
            {
                root = Path.Combine(root, parts[0]);
                parts.RemoveAt(0);
            }

            var directories = new List<string>();

            if (parts.Count > 0 && parts[0] == "**")
            {
                directories.AddRange(Directory.GetDirectories(root, "*", SearchOption.AllDirectories));
                parts.RemoveAt(0);
            }
            else if (parts.Count > 0)
            {
                directories.Add(root);
            }

            if (parts.Count != 1)
            {
                return false;
            }

            var pattern = parts[0];
            var files = new List<string>();

            foreach (var directory in directories)
            {
                files.AddRange(Directory.GetFiles(directory, pattern));
            }

            foreach (var file in files)
            {
                DeleteItem(file);
            }

            return true;
        }

        private static bool ContainsWildcard(string path) 
            => path != null && (path.Contains("*") || path.Contains("?"));

        public bool DeleteItem(string path)
        {
            if (ContainsWildcard(path))
            {
                return DeleteWildcard(path);
            }

            string fullPath = Path.Combine(Root, path);
            fullPath = Path.GetFullPath(fullPath);

            try
            {
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, recursive: true);
                }
                else if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
