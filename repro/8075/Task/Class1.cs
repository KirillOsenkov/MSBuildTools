using System;
using Microsoft.Build.Utilities;

namespace Task
{
    public class Class1 : ToolTask
    {
        protected override string ToolName => "SpecialSauce";

        protected override string GenerateFullPathToTool()
        {
            return @"C:\temp\msbuild_8075\silly\bin\Debug\net6.0\silly.exe";
        }

        public override bool Execute()
        {
            base.Execute();
            return base.Execute();
        }
    }
}
