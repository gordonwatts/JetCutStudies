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

        /// <summary>
        /// Cut for the log ratio when doing straight cuts.
        /// </summary>
        public static double LogRatioCut = 1.2;

        /// <summary>
        /// Cut in GeV for a track to violate isolation.
        /// </summary>
        public static double IsolationTrackPtCut = 2.0;

        /// <summary>
        /// How many tracks are allowed before isolation is violated?
        /// </summary>
        public static int IsolationTrackCountAllowed = 0;
    }
}
