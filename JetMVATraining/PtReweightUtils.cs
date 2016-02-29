using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static JetMVATraining.SampleUtils;
using static libDataAccess.Files;
using static libDataAccess.PlotSpecifications;

namespace JetMVATraining
{
    static class PtReweightUtils
    {
        /// <summary>
        /// Generate the pT spectra of teh sample.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IFutureValue<TH1F> PtSpectra (this IQueryable<JetStream> source)
        {
            return source
                .FuturePlot(JetPtPlot, "totalPlotSpectra");
        }

        /// <summary>
        /// Given a sequence, and its distribution, calculate a weight so it is flat.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="distribution"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public IQueryable<T> WeightToMakeFlat<T> (this IQueryable<T> source, ROOTNET.Interface.ITH1F distribution, Expression<Func<T,double> valueGetter, double desiredWeight = 1.0)
        {
            // First, invert the distribution to reweight.
            foreach (var bin in Enumerable.Range(0, distribution.NbinsX+1))
            {
                distribution.SetBinContent(bin, 1.0 / distribution.GetBinContent(bin));
                distribution.SetBinError(bin, 0.0);
            }
        }
    }
}
