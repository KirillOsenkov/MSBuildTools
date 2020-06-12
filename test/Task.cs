using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;

namespace HelloTask
{
    public class Hello : Task
    {
        public string YourName { get; set; }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "{0}", YourName);
            return true;
        }
    }
}
