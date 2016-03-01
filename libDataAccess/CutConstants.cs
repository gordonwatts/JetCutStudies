using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess
{
    /// <summary>
    /// Some cut constants for the various files - this is common.
    /// </summary>
    public static class CutConstants
    {
        /// <summary>
        /// The smallest distance that an LLP can decay and be considered signal for a CalRatio jet.
        /// Units are mm.
        /// </summary>
        public static double InnerDistanceForSignalLLPDecay = 2 * 1000;
    }
}
