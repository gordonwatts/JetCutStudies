using LinqToTTreeInterfacesLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    /// <summary>
    /// Helpful routines to dump strings to a stream.
    /// </summary>
    public static class FutureConsole
    {
        private static List<Func<string>> _lines = new List<Func<string>>();

        /// <summary>
        /// Evaluation of this function will generate a string. It is delayed until the
        /// appropriate time.
        /// </summary>
        /// <param name="futureString"></param>
        public static void FutureWriteLine (Func<string> futureString)
        {
            _lines.Add(futureString);
        }

        /// <summary>
        /// Write out a line...
        /// </summary>
        /// <param name="futureString"></param>
        public static void FutureWriteLine(IFutureValue<string> futureString)
        {
            _lines.Add(() => futureString.Value);
        }

        /// <summary>
        /// Dump the stream out
        /// </summary>
        public static void DumpToCout()
        {
            Console.Out.DumpFutureLines();
        }

        /// <summary>
        /// Dump everything to the console.
        /// </summary>
        /// <param name="output"></param>
        public static void DumpFutureLines(this TextWriter output)
        {
            _lines
                .ForEach(a => output.WriteLine(a()));
        }
    }
}
