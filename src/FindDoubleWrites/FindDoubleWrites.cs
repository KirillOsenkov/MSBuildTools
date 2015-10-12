using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
                        sb.AppendLine(string.Format("  {0} ({1} {2} {3})",
                            source,
                            new FileInfo(source).LastWriteTimeUtc,
                            new FileInfo(source).Length,
                            SHA1Hash(source)));
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
                .Select(f => f.LastWriteTimeUtc.ToString() + f.Length.ToString() + SHA1Hash(f.FullName))
                .Distinct()
                .Count() == 1)
            {
                return false;
            }

            return true;
        }

        public static string ByteArrayToHexString(byte[] bytes, int digits = 0)
        {
            if (digits == 0)
            {
                digits = bytes.Length * 2;
            }

            char[] c = new char[digits];
            byte b;
            for (int i = 0; i < digits / 2; i++)
            {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 87 : b + 0x30);
                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 87 : b + 0x30);
            }

            return new string(c);
        }

        private static string SHA1Hash(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var hash = new SHA1Managed())
            {
                var result = hash.ComputeHash(stream);
                return ByteArrayToHexString(result);
            }
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
