using System;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public class ErrorLogger : Logger
{
    public override void Initialize(IEventSource eventSource)
    {
        if (eventSource is IEventSource4 eventSource4)
        {
            eventSource4.IncludeEvaluationPropertiesAndItems();
        }

        eventSource.ErrorRaised += EventSource_ErrorRaised;
        eventSource.WarningRaised += EventSource_WarningRaised;
    }

    private void EventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
    {
        Log("Error", e.File, e.LineNumber, e.ColumnNumber, e.Code, e.Message);
    }

    private void EventSource_WarningRaised(object sender, BuildWarningEventArgs e)
    {
        Log("Warning", e.File, e.LineNumber, e.ColumnNumber, e.Code, e.Message);
    }

    private void Log(string type, string file, int line, int column, string code, string message) 
    {
        string text = $"##vso[task.logissue type={Escape(type)};sourcepath={Escape(file)};linenumber={line};columnnumber={column};code={Escape(code)};]{Escape(message)}";
        lock (this)
        {
            Console.WriteLine(text);
        }
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var result = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            switch (c)
            {
                case ';':
                    result.Append("%3B");
                    break;
                case '\r':
                    result.Append("%0D");
                    break;
                case '\n':
                    result.Append("%0A");
                    break;
                default:
                    result.Append(c);
                    break;
            }
        }

        return result.ToString();
    }
}