using CalRatioTMVAUtilities;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Linq;
using System.Linq.Expressions;
using static libDataAccess.PlotSpecifications;
using static LINQToTreeHelpers.PlottingUtils;
using static libDataAccess.Utils.FutureConsole;

namespace CalRatioTMVAUtilities
{
    public static class PtReweightUtils
    {
        /// <summary>
        /// How should we flatten the spectra we are looking at?
        /// </summary>
        public enum TrainingSpectraFlatteningPossibilities
        {
            JetPt,
            JetET,
            None
        }

        /// <summary>
        /// Generate a sequence that is flattened according to the function passed in.
        /// </summary>
        /// <param name="backgroundTrainingTree"></param>
        /// <param name="outputHistograms"></param>
        /// <param name="toMakeFlat">Function to flatten by. If null, then no flattening is done.</param>
        /// <returns></returns>
        public static IQueryable<TrainingTree> FlattenTrainingTree(IQueryable<TrainingTree> backgroundTrainingTree, FutureTFile outputHistograms, Expression<Func<TrainingTree, double>> toMakeFlat)
        {
            return toMakeFlat == null
                ? backgroundTrainingTree
                : backgroundTrainingTree
                    .FlattenBySpectra(toMakeFlat, outputHistograms, "background");
        }

        /// <summary>
        /// Given the flattening type, construct an expression that will do the flattening.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Expression<Func<TrainingTree, double>> BuildFlatteningExpression(TrainingSpectraFlatteningPossibilities options)
        {
            // Do flattening if requested
            switch (options)
            {
                case TrainingSpectraFlatteningPossibilities.JetPt:
                    FutureWriteLine("Reweighting to flatten as a function of JetPt");
                    return t => t.JetPt;
                case TrainingSpectraFlatteningPossibilities.JetET:
                    FutureWriteLine("Reweighting to flatten as a function of JetEt");
                    return t => t.JetET;
                case TrainingSpectraFlatteningPossibilities.None:
                    FutureWriteLine("Not reweighting at all before training.");
                    return null;
                default:
                    throw new InvalidOperationException("Unsupported flattening request.");
            }
        }

        /// <summary>
        /// Re weight a training tree by some given variable.
        /// </summary>
        /// <param name="source">Source of events</param>
        /// <param name="toFlattenBy">The expression we will flatten by.</param>
        /// <param name="output">Where we can put a plot showing what we have done</param>
        /// <param name="samplePrefix">How to name what we put out there</param>
        /// <returns></returns>
        public static IQueryable<TrainingTree> FlattenBySpectra(this IQueryable<TrainingTree> source, Expression<Func<TrainingTree, double>> toFlattenBy, FutureTDirectory output, string samplePrefix)
        {
            if (source == null)
            {
                return null;
            }

            // Make a before plot of the pT spectra.
            source
                .Select(j => Tuple.Create(toFlattenBy.Invoke(j), j.Weight))
                .FuturePlot<double>(JetPtPlot.NameFormat, JetPtPlot.TitleFormat, JetPtPlotRaw, samplePrefix)
                .Save(output);

            var r = source
                .ReweightToFlat(JetPtPlotRaw, t => toFlattenBy.Invoke(t), t => t.Weight, (t, w) => new TrainingTree()
                {
                    Weight = w,
                    CalRatio = t.CalRatio,
                    JetEta = t.JetEta,
                    JetPt = t.JetPt,
                    JetPhi = t.JetPhi,
                    MaxTrackPt = t.MaxTrackPt,
                    NTracks = t.NTracks,
                    SumPtOfAllTracks = t.SumPtOfAllTracks,
                    EventNumber = t.EventNumber,
                    JetET = t.JetET,
                    JetWidth = t.JetWidth,
                    JetDRTo2GeVTrack = t.JetDRTo2GeVTrack,
                    EnergyDensity = t.EnergyDensity,
                    FirstClusterRadius = t.FirstClusterRadius,
                    HadronicLayer1Fraction = t.HadronicLayer1Fraction,
                    JetLat = t.JetLat,
                    JetLong = t.JetLong,
                    ShowerCenter = t.ShowerCenter,
                    BIBDeltaTimingM = t.BIBDeltaTimingM,
                    BIBDeltaTimingP = t.BIBDeltaTimingP,
                });

            r
                .Select(j => Tuple.Create(toFlattenBy.Invoke(j), j.Weight))
                .FuturePlot(JetPtPlot.NameFormat, JetPtPlot.TitleFormat, JetPtPlotRaw, $"{samplePrefix}flat")
                .Save(output);

            return r;
        }

        private static int _plotIndex = 0;

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
                .FuturePlot($"bogus_name_{_plotIndex}", "bogus_title", plotter);
            _plotIndex++;

            // Re weight
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
