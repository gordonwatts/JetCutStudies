using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess
{
    /// <summary>
    /// Arrange to dump LINQToTTree messages to the console.
    /// </summary>
    public class ConsoleMessageDumper
    {
        public static void SetupConsoleMessageDumper()
        {
            TraceHelpers.Source.Switch = new SourceSwitch("console", "ActivityTracing");
            TraceHelpers.Source.Listeners.Add(new ConsoleTraceListener());
        }
    }
}
