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
        /// Write a string that doesn't need to be evaluated.
        /// </summary>
        /// <param name="constantLine"></param>
        public static void FutureWriteLine(string constantLine)
        {
            FutureWriteLine(() => constantLine);
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
        /// <remarks>Cache all the lines first, even if that is a lot of memory. Otherwise
        /// we end up with lots of computations in between.</remarks>
        public static void DumpFutureLines(this TextWriter output)
        {
            var lines = _lines
                .Select(a => a())
                .ToArray();

            lines
                .ForEach(a => output.WriteLine(a));
        }
    }
}
