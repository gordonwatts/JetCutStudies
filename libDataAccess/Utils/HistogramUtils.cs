using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    public static class HistogramUtils
    {
        /// <summary>
        /// Which direction should we scan bins from?
        /// </summary>
        public enum BinSearchOrder
        {
            LowestBin,
            HighestBin
        }

        /// <summary>
        /// find the first bin that is nonzero.
        /// </summary>
        /// <param name="histo"></param>
        /// <returns></returns>
        public static double FindNonZeroBinValue(this ROOTNET.Interface.NTH1 histo, BinSearchOrder order = BinSearchOrder.LowestBin)
        {
            var ibin = order == BinSearchOrder.LowestBin
                ? histo.FindFirstBinAbove()
                : histo.FindLastBinAbove();
            return histo.Xaxis.GetBinCenter(ibin);
        }
    }
}
