using System;
using System.Collections.Generic;
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
        private List<Func<string>> _lines = new List<Func<string>>();

        /// <summary>
        /// Evaluation of this func will generate a string. It is delayed until the
        /// appropriate time.
        /// </summary>
        /// <param name="futureString"></param>
        public static void WriteLine (Func<string> futureString)
        {
            _lines.Add(futureString);
        }
    }
}
