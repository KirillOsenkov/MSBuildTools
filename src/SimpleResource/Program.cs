using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        var assembly = typeof(Program).Assembly;
        var stream = assembly.GetManifestResourceStream(@"embedded.txt");

        var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        Console.WriteLine(text);
    }
}