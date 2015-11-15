using LinqToTTreeInterfacesLib;
using ROOTNET.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQToTreeHelpers.FutureUtils;

using static System.Math;

namespace GenericPerformancePlots
{
    static class PlotUtillities
    {
        /// <summary>
        /// Normalize the area under the plot to some value.
        /// </summary>
        /// <param name="fp">Plot to be normalized</param>
        /// <param name="area">The total area (defaults to 1.0)</param>
        /// <returns></returns>
        public static IFutureValue<NTH1> Normalize(this IFutureValue<NTH1> fp, double area = 1.0)
        {
            return fp.Select(p =>
            {
                p.SetNormFactor(area / p.Integral());
                return p;
            });
        }

        /// <summary>
        /// Calculate the squarter root of each bin, bin-by-bin.
        /// </summary>
        /// <param name="fp">Plot to take the sqrt</param>
        /// <returns></returns>
        public static IFutureValue<NTH1> Sqrt(this IFutureValue<NTH1> fp)
        {
            return fp.Select(p =>
            {
                var newp = p.Clone(p.Name + "_sqrt") as NTH1;
                foreach (var bin in Enumerable.Range(0, p.NbinsX+1))
                {
                    var err = newp.GetBinError(bin);
                    var val = newp.GetBinContent(bin);
                    var newval = Math.Sqrt(val);
                    var newerr = err / val * newval;
                    newp.SetBinContent(bin, newval);
                    newp.SetBinError(bin, newerr);
                }
                return newp;
            });
        }

        /// <summary>
        /// Divide the first by the second histogram
        /// </summary>
        /// <param name="hnum"></param>
        /// <param name="denom"></param>
        /// <returns></returns>
        public static IFutureValue<NTH1> DividedBy(this IFutureValue<NTH1> hnum, IFutureValue<NTH1> denom)
        {
            return from hn in hnum
                   from hd in denom
                   select InternalDivide(hn, hd);
        }

        /// <summary>
        /// Errors that come from doing various histogram operations.
        /// </summary>
        [Serializable]
        public class HistogramOperationErrorException : Exception
        {
            public HistogramOperationErrorException() { }
            public HistogramOperationErrorException(string message) : base(message) { }
            public HistogramOperationErrorException(string message, Exception inner) : base(message, inner) { }
            protected HistogramOperationErrorException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context)
            { }
        }

        /// <summary>
        /// Internally do the divide, hidden in the monad.
        /// </summary>
        /// <param name="hn"></param>
        /// <param name="hd"></param>
        /// <returns></returns>
        private static NTH1 InternalDivide(NTH1 hn, NTH1 hd)
        {
            var result = hn.Clone(string.Format("{0}_div_{1}", hn.Name, hd.Name)) as NTH1;
            if (!result.Divide(hd))
            {
                throw new HistogramOperationErrorException($"Failure to divide {hn.Name} by {hd.Name}");
            }
            return result;
        }

        /// <summary>
        /// Do an in-place rename of the histogram.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static IFutureValue<NTH1> Rename(this IFutureValue<NTH1> val, string newName)
        {
            return val.Select(h => { h.Name = newName; return h; });
        }

        /// <summary>
        /// Given the current distribution, make it a cumulative one, respecting errors
        /// and also underflow and overflow bins, and taking into acount the bin width.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static IFutureValue<NTH1> AsCumulative(this IFutureValue<NTH1> val)
        {
            return val.Select(h =>
            {
                var hcum = h.Clone($"{h.Name}_cumulative") as NTH1;
                var integral = hcum.Integral();
                hcum.NormFactor = 0.0;

                double runningError2 = 0;
                double runningSum = 0;
                foreach (var ibin in Enumerable.Range(0, hcum.NbinsX+1))
                {
                    var v = hcum.GetBinContent(ibin);
                    var e = hcum.GetBinError(ibin);
                    //var width = hcum.GetBinWidth(ibin);

                    runningSum += v/integral;
                    runningError2 += e*e/integral/integral;

                    hcum.SetBinContent(ibin, runningSum);
                    hcum.SetBinError(ibin, Math.Sqrt(runningError2));
                }

                return hcum;
            });
        }
    }
}
