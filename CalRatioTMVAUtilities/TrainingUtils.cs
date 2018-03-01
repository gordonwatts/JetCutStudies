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
using libDataAccess;

namespace CalRatioTMVAUtilities
{
    /// <summary>
    /// The info we will hand to the training
    /// </summary>
    public class TrainingTree
    {
        public int RunNumber;

        /// <summary>
        /// The total weight - includes:
        /// - MC Event Weight
        /// - x-section weight (if any)
        /// - flattening weight (if any)
        /// </summary>
        public double Weight;

        /// <summary>
        /// The weight from the original MC event (only)
        /// </summary>
        public double WeightMCEvent;

        /// <summary>
        /// The cross section weight (only)
        /// </summary>
        public double WeightXSection;

        /// <summary>
        /// The weight for flattening the pT (or whatever) for this event
        /// </summary>
        public double WeightFlatten;

        /// <summary>
        /// The MC value of Lxy for this event. Zero if not associated.
        /// </summary>
        public double mc_Lxy;

        /// <summary>
        /// The Lz value of this event, zero if not known.
        /// </summary>
        public double mc_Lz;

        /// <summary>
        /// The missing HT divided by the HT. A global event variable cut
        /// </summary>
        public double MHTOverHT;

        public double JetPt;
        public double JetPhi;
        public double CalRatio;
        public double JetEta;
        public int NTracks;
        public double SumPtOfAllTracks;
        public double MaxTrackPt;
        public int EventNumber;
        public double JetET;
        public double JetWidth;
        public double JetDRTo2GeVTrack; // Not used by default any longer
        public double EnergyDensity;
        public double HadronicLayer1Fraction;
        public double JetLat;
        public double JetLong;
        public double FirstClusterRadius;
        public double NumberOfClusters;
        public double ShowerCenter;
        public double BIBDeltaTimingM;
        public double BIBDeltaTimingP;
        public double FirstCellTiming;

        public double InteractionsPerCrossing;

        // All the different versions of the MLP stuff
        // First, Rachels MLP stuff
        public double RPredictedLxy;
        public double RPredictedLz;

        // Felixes
        public double PredictedLxy;
        public double PredictedLz;

        // Felixes high and low eta stuff
        public double PredictedLxyHighEta;
        public double PredictedLxyLowEta;
        public double PredictedLzHighEta;
        public double PredictedLzLowEta;
    }

    /// <summary>
    /// Utility functions to setup the training.
    /// </summary>
    public static class TrainingUtils
    {
        /// <summary>
        /// Converter expression that can be used in our LINQ queries
        /// </summary>
        public static Expression<Func<JetStream, TrainingTree>> TrainingTreeConverter = i
            => new TrainingTree()
            {
                Weight = i.Weight,
                WeightFlatten = i.WeightFlatten,
                WeightMCEvent = i.WeightMCEvent,
                WeightXSection = i.WeightXSection,
                CalRatio = NormalizeCalRatio.Invoke(i.JetInfo.Jet.logRatio),
                JetPt = i.JetInfo.Jet.pT,
                JetEta = i.JetInfo.Jet.eta,
                JetPhi = i.JetInfo.Jet.phi,
                //NTracks = i.JetInfo.Tracks.Count(),
                NTracks = (int) i.JetInfo.Jet.nTrk,
                //SumPtOfAllTracks = i.JetInfo.AllTracks.Sum(t => t.pT),
                SumPtOfAllTracks = i.JetInfo.Jet.CalibJet_sumTrkpT,
                //MaxTrackPt = CalcMaxPt.Invoke(i.JetInfo.AllTracks),
                MaxTrackPt = i.JetInfo.Jet.CalibJet_maxTrkpT,
                EventNumber = i.EventNumber,
                RunNumber = i.RunNumber,
                MHTOverHT = i.MHTOverHT,
                JetET = i.JetInfo.Jet.ET,
                JetWidth = i.JetInfo.Jet.width,
                //JetDRTo2GeVTrack = PlotSpecifications.CalcDR2GeVTrack.Invoke(i.JetInfo.AllTracks, i.JetInfo.Jet),
                JetDRTo2GeVTrack = i.JetInfo.Jet.CalibJet_minDRTrkpt2,
                JetLat = i.JetInfo.Jet.FirstClusterLateral,
                JetLong = i.JetInfo.Jet.FirstClusterLongitudinal,
                FirstClusterRadius = i.JetInfo.Jet.FirstClusterR,
                NumberOfClusters = i.JetInfo.Jet.CalibJet_cluster_nClusters,
                ShowerCenter = i.JetInfo.Jet.FirstClusterLambda,
                EnergyDensity = i.JetInfo.Jet.FirstClusterEnergyDensity,
                HadronicLayer1Fraction = i.JetInfo.Jet.EHL1frac,
                BIBDeltaTimingM = PlotSpecifications.CalcBIBMinusDeltaPlotTiming.Invoke(i.JetInfo.Jet),
                BIBDeltaTimingP = PlotSpecifications.CalcBIBPlusDeltaPlotTiming.Invoke(i.JetInfo.Jet),
                FirstCellTiming = i.JetInfo.Jet.time,
                InteractionsPerCrossing = i.InteractionsPerCrossing,

                PredictedLxy = i.JetInfo.Jet.Predicted_Lxy,
                PredictedLz = i.JetInfo.Jet.Predicted_Lz,
                RPredictedLxy = i.JetInfo.Jet.Rachel_Predicted_Lxy,
                RPredictedLz = i.JetInfo.Jet.Rachel_Predicted_Lz,

                PredictedLxyHighEta = i.JetInfo.Jet.Predicted_Lxy_highEta,
                PredictedLxyLowEta = i.JetInfo.Jet.Predicted_Lxy_lowEta,
                PredictedLzHighEta = i.JetInfo.Jet.Predicted_Lz_highEta,
                PredictedLzLowEta = i.JetInfo.Jet.Predicted_Lz_lowEta,

                mc_Lxy = i.JetInfo.Jet.LLP.IsGoodIndex() ? i.JetInfo.Jet.LLP.Lxy / 1000.0 : 0.0,
                mc_Lz = i.JetInfo.Jet.LLP.IsGoodIndex() ? i.JetInfo.Jet.LLP.Lz / 1000.0 : 0.0,
            };

        /// <summary>
        /// Remove training events
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<TrainingTree> FilterNonTrainingEvents (this IQueryable<TrainingTree> source)
        {
            return source?.Where(t => t.EventNumber % 2 == 0);
        }

        /// <summary>
        /// Keep only the training events
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<TrainingTree> FilterTrainingEvents(this IQueryable<TrainingTree> source)
        {
            return source.Where(t => t.EventNumber % 2 == 1);
        }

        /// <summary>
        /// Max JetPt cut we will train on.
        /// </summary>
        public const double MaxJetPtForTraining = 550.0;

        /// <summary>
        /// Create a training tree from a jet stream.
        /// Apply training cleanup cuts.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<TrainingTree> AsTrainingTree(this IQueryable<JetStream> source)
        {
            return source?.Where(j => j.JetInfo.Jet.pT < MaxJetPtForTraining)
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
            new PlotInfo() { Plotter = JetPhiPlotRaw, ValueGetter = tu => tu.JetPhi },
            new PlotInfo() { Plotter = JetETPlotRaw, ValueGetter = tu => tu.JetET },
            new PlotInfo() { Plotter = JetEtaPlotRaw, ValueGetter = tu => tu.JetEta },
            new PlotInfo() { Plotter = JetCalRPlotRaw, ValueGetter = tu => tu.CalRatio },
            new PlotInfo() { Plotter = TrainingEventWeight, ValueGetter = tu => tu.Weight },
            new PlotInfo() { Plotter = NTrackPlotRaw, ValueGetter = tu => tu.NTracks },
            new PlotInfo() { Plotter = SumTrackPtPlotRaw, ValueGetter = tu => tu.SumPtOfAllTracks },
            new PlotInfo() { Plotter = MaxTrackPtPlotRaw, ValueGetter = tu => tu.MaxTrackPt },
            new PlotInfo() { Plotter = JetWidthPlotRaw, ValueGetter = tu => tu.JetWidth },
            new PlotInfo() { Plotter = DeltaROfCloseTrackPlotRaw, ValueGetter = tu => tu.JetDRTo2GeVTrack },
            new PlotInfo() { Plotter = EnergyDensityPlotRaw, ValueGetter = tu => tu.EnergyDensity },
            new PlotInfo() { Plotter = HadronicL1FractPlotRaw, ValueGetter = tu => tu.HadronicLayer1Fraction },
            new PlotInfo() { Plotter = JetLatPlotRaw, ValueGetter = tu => tu.JetLat },
            new PlotInfo() { Plotter = JetLongPlotRaw, ValueGetter = tu => tu.JetLong },
            new PlotInfo() { Plotter = FirstClusterRadiusPlotRaw, ValueGetter = tu => tu.FirstClusterRadius },
            new PlotInfo() { Plotter = ShowerCenterPlotRaw, ValueGetter = tu => tu.ShowerCenter },
            new PlotInfo() { Plotter = BIBDeltaPlusTimingPlotRaw, ValueGetter = tu => tu.BIBDeltaTimingP },
            new PlotInfo() { Plotter = BIBDeltaMinusTimingPlotRaw, ValueGetter = tu => tu.BIBDeltaTimingM },
            new PlotInfo() { Plotter = JetCalPredictedLzPlotRaw, ValueGetter = tu => tu.PredictedLz / 1000.0},
            new PlotInfo() { Plotter = JetCalPredictedLxyPlotRaw, ValueGetter = tu => tu.PredictedLxy / 1000.0},
            new PlotInfo() { Plotter = EventInteractionsPerCossing, ValueGetter = tu => tu.InteractionsPerCrossing},
        };

        /// <summary>
        /// Make plots of everything
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<TrainingTree> PlotTrainingVariables (this IQueryable<TrainingTree> source, FutureTDirectory dir, string tag)
        {
            if (source == null)
            {
                return null;
            }

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
        /// <param name="source">Source of events to look for NN cut on</param>
        /// <param name="passFraction">Find the cut for the fraction of events that should pass</param>
        /// <param name="dir">Directory to write output to</param>
        /// <param name="classIndex">For a mutli-class classifier, what class should we look at</param>
        /// <param name="method">Method that made this classifier</param>
        /// <param name="name">The name of the histogram to cache</param>
        /// <returns></returns>
        public static IFutureValue<double> FindNNCut(this IQueryable<TrainingTree> source, double passFraction, FutureTDirectory dir, Method<TrainingTree> method,
            int classIndex = 0, string name = "")
        {
            if (passFraction < 0 || passFraction > 1.0)
            {
                throw new ArgumentException($"passFraction of {passFraction} is not between 0 and 1 - not legal!");
            }

            // dump the MVA output into a large histogram that has lots of bins so we can calculate.
            var p = source
                .MakeNNPlot(method, classIndex: classIndex, name: name)
                .Save(dir);

            // Now, look through the plot, bin by bin, till we get past the total.
            var bin = from h in p select CalcBinWhereFractionIs(h, passFraction);

            // And the center of that bin
            var binCenter = from b in bin from h in p select h.GetBinCenter(b);

            return binCenter;
        }

        /// <summary>
        /// Calculate the training error (expected_value - wt)^2 average for a particular sample.
        /// </summary>
        /// <param name="source">Training events we are going to look at</param>
        /// <param name="method">The TMVA method to look at these items</param>
        /// <returns></returns>
        public static IFutureValue<double> CalcTrainingError(this IQueryable<TrainingTree> source, Method<TrainingTree> method, int classIndex, double expectedValue)
        {
            // Quick checks.
            if (!method.WeightFile.Exists)
            {
                throw new ArgumentException($"File {method.WeightFile.FullName} can't be located.");
            }
            if (!method.IsMultiClass && classIndex != 0)
            {
                throw new ArgumentException("Can't specify a class index if multi-class is set to false.");
            }

            // Get the sum and the count, and then the average.
            var count = source.FutureCount();

            var cBDT = method.GetMVAMulticlassValue();
            var mvaCalc = method.IsMultiClass
                ? t => (double)(cBDT.Invoke(t)[classIndex])
                : method.GetMVAValue();
            var sum = source.Select(e => (expectedValue - mvaCalc.Invoke(e)) * (expectedValue - mvaCalc.Invoke(e))).FutureSum();

            return from c in count from s in sum select (c == 0 ? 0.0 : (s / c));
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
        /// <param name="classIndex">Which MVA class should it return</param>
        /// <param name="method">Method that the training dwas done for.</param>
        /// <param name="name">Name of the histogram to cache</param>
        /// <returns></returns>
        internal static IFutureValue<ROOTNET.NTH1F> MakeNNPlot (this IQueryable<TrainingTree> source, Method<TrainingTree> method,
            int classIndex = 0, string name = "")
        {
            // Argument check
            if (!method.WeightFile.Exists)
            {
                throw new ArgumentException($"File {method.WeightFile.FullName} can't be located.");
            }
            if (!method.IsMultiClass && classIndex != 0)
            {
                throw new ArgumentException("Can't specify a class index if multi-class is set to false.");
            }

            // If we are a multi-class, get back the proper class response. Otherwise,
            // get back the single type of classifier output.
            var cBDT = method.GetMVAMulticlassValue();
            var mvaCalc = method.IsMultiClass
                ? t => (double)(cBDT.Invoke(t)[classIndex])
                : method.GetMVAValue();

            // Generate a pretty detailed plot
            return source
                .FuturePlot($"mva_weights{name}", $"MVA Output Weights {name}", 10000, -1.0, 1.0, t => mvaCalc.Invoke(t), weight: t => t.Weight);
        }
    }
}
