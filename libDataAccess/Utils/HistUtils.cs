using ROOTNET.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Linq.Enumerable;

namespace libDataAccess.Utils
{
    /// <summary>
    /// Utilities for manipulating histograms in nice tight ways.
    /// </summary>
    public static class HistUtils
    {
        /// <summary>
        /// Return the data in the histogram as an array
        /// </summary>
        /// <param name="hist"></param>
        /// <returns></returns>
        public static double[] Data(this NTH1 hist)
        {
            return Range(0, hist.NbinsX + 1).Select(idx => hist.GetBinContent(idx)).ToArray();
        }
    }
}
