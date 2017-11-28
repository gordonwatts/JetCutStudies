using LINQToTTreeLib;
using System.Diagnostics;

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
            TraceHelpers.Source.Listeners.Add(new ConsoleTraceListener() { IndentLevel=2, IndentSize = 2, TraceOutputOptions = TraceOptions.None });
        }
    }
}
