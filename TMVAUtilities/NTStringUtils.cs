using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMVAUtilities
{
    static class NTStringUtils
    {
        /// <summary>
        /// Convert a string to a TString class.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ROOTNET.Interface.NTString AsTS (this string source)
        {
            return new ROOTNET.NTString(source);
        }
    }
}
