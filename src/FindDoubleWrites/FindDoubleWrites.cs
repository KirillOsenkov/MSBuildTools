using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FindDoubleWrites
{
    internal class FindDoubleWrites
    {
        private static readonly Dictionary<string, HashSet<string>> sourcesForDestination = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        private static void Main(string[] args)
        {
            var copylogs = Directory.GetFiles(Environment.CurrentDirectory, "copylog.txt", SearchOption.AllDirectories);
            foreach (var copylog in copylogs)
            {
                ProcessCopyLog(copylog);
            }

            var sb = new StringBuilder();
            foreach (var bucket in sourcesForDestination)
            {
                if (IsDoubleWrite(bucket))
                {
                    sb.AppendLine("Double-write for file: " + bucket.Key);

                    foreach (var source in bucket.Value)
                    {
                        sb.AppendLine("  " + source);
                    }

                    sb.AppendLine();
                }
            }

            File.WriteAllText("report.txt", sb.ToString());
        }

        private static bool IsDoubleWrite(KeyValuePair<string, HashSet<string>> bucket)
        {
            if (bucket.Value.Count < 2)
            {
                return false;
            }

            if (bucket.Value
                .Select(f => new FileInfo(f))
                .Select(f => f.LastWriteTimeUtc.ToString() + f.Length.ToString())
                .Distinct()
                .Count() == 1)
            {
                return false;
            }

            return true;
        }

        private static void ProcessCopyLog(string copylog)
        {
            var lines = File.ReadAllLines(copylog);
            for (int i = 0; i < lines.Length; i += 2)
            {
                var source = lines[i];
                var destination = lines[i + 1];
                ProcessCopy(source, destination);
            }
        }

        private static void ProcessCopy(string source, string destination)
        {
            HashSet<string> bucket = null;
            if (!sourcesForDestination.TryGetValue(destination, out bucket))
            {
                bucket = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                sourcesForDestination.Add(destination, bucket);
            }

            bucket.Add(source);
        }
    }
}
