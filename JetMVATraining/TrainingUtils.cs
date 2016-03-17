﻿using LINQToTreeHelpers;
using LINQToTTreeLib;
using LINQToTreeHelpers.FutureUtils;
using System;
using System.Linq;
using System.Linq.Expressions;
using static JetMVATraining.SampleUtils;
using static libDataAccess.PlotSpecifications;
using static LINQToTreeHelpers.PlottingUtils;
using LINQToTTreeLib.CodeAttributes;
using LinqToTTreeInterfacesLib;
using System.IO;
using ROOTNET;

namespace JetMVATraining
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
    }

    /// <summary>
    /// Utility functions to setup the training.
    /// </summary>
    public static class TrainingUtils
    {
        internal static Expression<Func<JetStream, TrainingTree>> TrainingTreeConverter = i
            => new TrainingTree()
            {
                Weight = i.Weight,
                CalRatio = NormalizeCalRatio.Invoke(i.JetInfo.Jet.logRatio),
                JetPt = i.JetInfo.Jet.pT,
                JetEta = i.JetInfo.Jet.eta,
                NTracks = i.JetInfo.Tracks.Count(),
                SumPtOfAllTracks = i.JetInfo.AllTracks.Sum(t => t.pT),
                MaxTrackPt = CalcMaxPt.Invoke(i.JetInfo.AllTracks),
            };

        /// <summary>
        /// Create a training tree from a jet stream.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static IQueryable<TrainingTree> AsTrainingTree(this IQueryable<JetStream> source)
        {
            return source
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
        internal static IQueryable<TrainingTree> PlotTrainingVariables (this IQueryable<TrainingTree> source, FutureTDirectory dir, string tag)
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
        internal static double FindNNCut(this IQueryable<TrainingTree> source, double passFraction, FutureTDirectory dir, FileInfo weightFile)
        {
            if (passFraction < 0 || passFraction > 1.0)
            {
                throw new ArgumentException($"passFraction of {passFraction} is not between 0 and 1 - not legal!");
            }

            // dump the MVA output into a large histogram that has lots of bins so we can calculate.
            var p = source
                .MakeNNPlot(weightFile)
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
        /// Calculate the MVA
        /// </summary>
        internal static Expression<Func<TrainingTree, FileInfo, double>> CalculateMVA = (t, weightFile) => TMVAReaderHelpers.TMVASelectorJetBDT(t.JetPt, t.CalRatio, t.JetEta, t.NTracks, t.SumPtOfAllTracks, t.MaxTrackPt, weightFile.FullName.Escape());

        /// <summary>
        /// Build a nice plot of the training value
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static IFutureValue<ROOTNET.NTH1F> MakeNNPlot (this IQueryable<TrainingTree> source, FileInfo weightFile)
        {
            // Argument check
            if (!weightFile.Exists)
            {
                throw new ArgumentException($"File {weightFile.FullName} can't be located.");
            }

            // Generate a pretty detailed plot
            return source
                .FuturePlot("mva_weights", "MVA Output Weights", 1000, -1.0, 1.0, t => CalculateMVA.Invoke(t, weightFile));
        }

        private static string Escape(this string s)
        {
            return s.Replace("\\", "\\\\");
        }
    }

    [CPPHelperClass]
    public class TMVAReaderHelpers
    {
        [CPPCode(IncludeFiles = new[] { "tmva/Reader.h" },
            Code = new[] {
                "static bool initUnique = false;",
                "static float JetPtUnique = 0.0;",
                "static float CalRatioUnique = 0.0;",
                "static float JetEtaUnique = 0.0;",
                "static float nTracksUnique = 0.0;",
                "static float SumPtOfAllTracksUnique = 0.0;",
                "static float MaxTrackPtUnique = 0.0;",
                "static TMVA::Reader *readerUnique = 0;",
                "if (!initUnique) {",
                "  initUnique = true;",
                "  readerUnique = new TMVA::Reader();",
                "  readerUnique->AddVariable(\"JetPt\", &JetPtUnique);",
                "  readerUnique->AddVariable(\"CalRatio\", &CalRatioUnique);",
                "  readerUnique->AddVariable(\"JetEta\", &JetEtaUnique);",
                "  readerUnique->AddVariable(\"NTracks\", &nTracksUnique);",
                "  readerUnique->AddVariable(\"SumPtOfAllTracks\", &SumPtOfAllTracksUnique);",
                "  readerUnique->AddVariable(\"MaxTrackPt\", &MaxTrackPtUnique);",
                "  readerUnique->BookMVA(\"SimpleBDT\", weightName);",
                "}",
                "JetPtUnique = jetPTa;",
                "CalRatioUnique = CalRatioa;",
                "JetEtaUnique = JetEtaa;",
                "nTracksUnique = nTracksa;",
                "SumPtOfAllTracksUnique = SumPta;",
                "MaxTrackPtUnique = maxTrackPta;",
                "TMVASelectorJetBDT = readerUnique->EvaluateMVA(\"SimpleBDT\");"
            })]
        public static double TMVASelectorJetBDT(double jetPTa, double CalRatioa, double JetEtaa,
            int nTracksa, double SumPta, double maxTrackPta, string weightName)
        {
            throw new NotImplementedException("This should never get called!");
        }
    }
}
