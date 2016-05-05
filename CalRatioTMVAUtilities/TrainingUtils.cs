using LINQToTreeHelpers;
using LINQToTTreeLib;
using LINQToTreeHelpers.FutureUtils;
using System;
using System.Linq;
using System.Linq.Expressions;
using static libDataAccess.PlotSpecifications;
using static LINQToTreeHelpers.PlottingUtils;
using LinqToTTreeInterfacesLib;
using ROOTNET;
using libDataAccess.Utils;
using TMVAUtilities;

namespace CalRatioTMVAUtilities
{
    /// <summary>
    /// The info we will hand to the training
    /// </summary>
    public class TrainingTree
    {
        public double Weight;
        public double JetPt;
        public double CalRatio;
        public double JetEta;
        public int NTracks;
        public double SumPtOfAllTracks;
        public double MaxTrackPt;
        public int EventNumber;
        public double JetET;
    }

    /// <summary>
    /// Utility functions to setup the training.
    /// </summary>
    public static class TrainingUtils
    {
        public static Expression<Func<JetStream, TrainingTree>> TrainingTreeConverter = i
            => new TrainingTree()
            {
                Weight = i.Weight,
                CalRatio = NormalizeCalRatio.Invoke(i.JetInfo.Jet.logRatio),
                JetPt = i.JetInfo.Jet.pT,
                JetEta = i.JetInfo.Jet.eta,
                NTracks = i.JetInfo.Tracks.Count(),
                SumPtOfAllTracks = i.JetInfo.AllTracks.Sum(t => t.pT),
                MaxTrackPt = CalcMaxPt.Invoke(i.JetInfo.AllTracks),
                EventNumber = i.EventNumber,
                JetET = i.JetInfo.Jet.ET,
            };

        /// <summary>
        /// Create a training tree from a jet stream.
        /// Apply training cleanup cuts.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<TrainingTree> AsTrainingTree(this IQueryable<JetStream> source)
        {
            return source
                .Where(j => j.JetInfo.Jet.pT < 400.0)
                .Select(i => TrainingTreeConverter.Invoke(i));
        }

        private class PlotInfo
        {
            public IPlotSpec<double> Plotter;
            public Expression<Func<TrainingTree, double>> ValueGetter;
        }

        /// <summary>
        /// List of ways to plot all the training variables.
        /// </summary>
        private static PlotInfo[] _plotters = new PlotInfo[]
        {
            new PlotInfo() { Plotter = JetPtPlotRaw, ValueGetter = tu => tu.JetPt },
            new PlotInfo() { Plotter = JetEtaPlotRaw, ValueGetter = tu => tu.JetEta },
            new PlotInfo() { Plotter = JetCalRPlotRaw, ValueGetter = tu => tu.CalRatio },
            new PlotInfo() { Plotter = TrainingEventWeight, ValueGetter = tu => tu.Weight },
            new PlotInfo() { Plotter = NTrackPlotRaw, ValueGetter = tu => tu.NTracks },
            new PlotInfo() { Plotter = SumTrackPtPlotRaw, ValueGetter = tu => tu.SumPtOfAllTracks },
            new PlotInfo() { Plotter = MaxTrackPtPlotRaw, ValueGetter = tu => tu.MaxTrackPt },
        };

        /// <summary>
        /// Make plots of everything
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<TrainingTree> PlotTrainingVariables (this IQueryable<TrainingTree> source, FutureTDirectory dir, string tag)
        {
            // Plots of all the items
            foreach (var p in _plotters)
            {
                source
                    .Select(j => Tuple.Create(p.ValueGetter.Invoke(j), j.Weight))
                    .FuturePlot(p.Plotter.NameFormat, p.Plotter.TitleFormat, p.Plotter, tag)
                    .Save(dir);

                source
                    .Select(j => p.ValueGetter.Invoke(j))
                    .FuturePlot(p.Plotter, $"{tag}_unweighted")
                    .Save(dir);
            }

            // Continue on...
            return source;
        }

        /// <summary>
        /// Determine the NN value for a pass value. Return it.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="passFraction"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static double FindNNCut(this IQueryable<TrainingTree> source, double passFraction, FutureTDirectory dir, Method<TrainingTree> m)
        {
            if (passFraction < 0 || passFraction > 1.0)
            {
                throw new ArgumentException($"passFraction of {passFraction} is not between 0 and 1 - not legal!");
            }

            // dump the MVA output into a large histogram that has lots of bins so we can calculate.
            var p = source
                .MakeNNPlot(m)
                .Save(dir);

            // Now, look through the plot, bin by bin, till we get past the total.
            var bin = from h in p select CalcBinWhereFractionIs(h, passFraction);

            // And the center of that bin
            var binCenter = from b in bin from h in p select h.GetBinCenter(b);

            return binCenter.Value;
        }

        /// <summary>
        /// Calculate the pass fraction for a bin.
        /// </summary>
        /// <param name="h">Histogram we are to examine (assume 1D)</param>
        /// <param name="passFraction">The fraction for passing - assume it is between 0 and 1.</param>
        /// <returns></returns>
        private static int CalcBinWhereFractionIs(NTH1F h, double passFraction)
        {
            var sum = Enumerable.Range(0, h.GetNbinsX() + 1).Select(b => h.GetBinContent(b)).Sum();
            var sumToPassFraction = sum * passFraction;

            var incSum = 0.0;
            foreach (var b in Enumerable.Range(0, h.GetNbinsX() + 1))
            {
                incSum += h.GetBinContent(b);
                if (incSum > sumToPassFraction)
                {
                    return b;
                }
            }

            // If we got here, then the number must have been 1, and it is the last
            // bin that we return!
            return h.GetNbinsX() + 1;
        }

        /// <summary>
        /// Build a nice plot of the training value
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static IFutureValue<ROOTNET.NTH1F> MakeNNPlot (this IQueryable<TrainingTree> source, Method<TrainingTree> m)
        {
            // Argument check
            if (!m.WeightFile.Exists)
            {
                throw new ArgumentException($"File {m.WeightFile.FullName} can't be located.");
            }

            // Generate a pretty detailed plot
            var mvaCalc = m.GetMVAValue();
            return source
                .FuturePlot("mva_weights", "MVA Output Weights", 10000, -1.0, 1.0, t => mvaCalc.Invoke(t), weight: t => t.Weight);
        }
    }
}
