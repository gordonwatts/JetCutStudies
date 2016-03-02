using DiVertAnalysis;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Linq;
using System.Linq.Expressions;
using static JetMVATraining.SampleUtils;
using static libDataAccess.PlotSpecifications;
using static LINQToTreeHelpers.PlottingUtils;

namespace JetMVATraining
{
    static class PtReweightUtils
    {
        /// <summary>
        /// Return a stream that is flattened in pT. Produce some before and after plots.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static IQueryable<JetStream> FlattenPtSpectra (this IQueryable<JetStream> source, FutureTDirectory output, string samplePrefix)
        {
            // Make a before plot of the pT spectra.
            source
                .Select(j => Tuple.Create(j.JetInfo.Jet, j.Weight))
                .FuturePlot<recoTreeJets>(JetPtPlot.NameFormat, JetPtPlot.TitleFormat, JetPtPlot, samplePrefix)
                .Save(output);

            var r = source
                .ReweightToFlat(JetPtPlot, t => t.JetInfo.Jet, t => t.Weight, (t, w) => new JetStream() { JetInfo = t.JetInfo, Weight = w });

            r
                .Select(j => Tuple.Create(j.JetInfo.Jet, j.Weight))
                .FuturePlot(JetPtPlot.NameFormat, JetPtPlot.TitleFormat, JetPtPlot, $"{samplePrefix}flat")
                .Save(output);

            return r;

        }

        /// <summary>
        /// Will make a sequence flat in some plotting parameter.
        /// It should work for a 2D distribution as well as a 1D one, but it hasn't been tested yet.
        /// </summary>
        /// <typeparam name="T">The type of the sequence to be re-weighted</typeparam>
        /// <typeparam name="U">The type of the sequence to be plotted</typeparam>
        /// <param name="source">The sequence to be re-weighted</param>
        /// <param name="plotter">The plot spec that will generate the re-weighting plot</param>
        /// <param name="converter">Convert the incoming sequence to a type the plotter can deal with</param>
        /// <param name="weight">Convert the incoming sequence to a weight</param>
        /// <param name="builder">rebuild the incoming sequence, with a new over all weight</param>
        /// <param name="normalization">The normalization of the final sequence (defaults to one)</param>
        /// <returns></returns>
        public static IQueryable<T> ReweightToFlat<T,U>(this IQueryable<T> source, IPlotSpec<U> plotter, Expression<Func<T,U>> converter, Expression<Func<T,double>> weight, Expression<Func<T, double, T>> builder, double normalization = 1.0)
        {
            // First, get the spectra. We will process that into a re-weighting.
            var ptSpecra = source
                .Select(t => Tuple.Create(converter.Invoke(t), weight.Invoke(t)))
                .FuturePlot("bogus_name", "bogus_title", plotter);

            // Reweight
            var reweightSpectr = ptSpecra.Value;
            foreach (var b in Enumerable.Range(0, reweightSpectr.NbinsX + 1))
            {
                var v = reweightSpectr.GetBinContent(b);
                var newV = v == 0 ? 0 : normalization / reweightSpectr.GetBinContent(b);
                reweightSpectr.SetBinContent(b, newV);
                reweightSpectr.SetBinError(b, 0.0);
            }

            // Now, generate an updated sequence that properly does the re-weighting.
            return source
                .Select(t => builder.Invoke(t, weight.Invoke(t) * reweightSpectr.GetBinContent(plotter.Bin.Invoke(converter.Invoke(t), reweightSpectr))));
        }
    }
}
