﻿using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LinqToTTreeInterfacesLib;
using LINQToTTreeLib.Files;
using LINQToTTreeLib;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using static JetMVATraining.SampleUtils;
using static libDataAccess.PlotSpecifications;
using ROOTNET.Interface;
using static LINQToTreeHelpers.PlottingUtils;
using System.Collections.Generic;
using DiVertAnalysis;
using TMVAUtilities;
using static libDataAccess.Utils.FutureConsole;

namespace JetMVATraining
{
    class Program
    {
        /// <summary>
        /// Total number of background events to use. It will be split equally amongst all JZ samples.
        /// </summary>
        const int NumberOfBackgroundEvents = 250000;

        /// <summary>
        /// Run the training for the MVA. This is run in a library,
        /// so basically behind-this-guys-back. But it provides an easy single item to run.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Parse command line arguments
            CommandLineUtils.Parse(args);

            // Get the background data sources, and figure out how many per source we want to grab.
            var backgroundSources = CommandLineUtils.GetRequestedBackgroundSourceList();
            var numberSources = backgroundSources.Count();
            var numberOfEventsPerSource = NumberOfBackgroundEvents / numberSources;

            // Our data sources
            var background = CommandLineUtils.GetRequestedBackground()
                .AsGoodJetStream()
                .TakePerSource(numberOfEventsPerSource);

            var signal = (Files.Get600pi150lt9m().Concat(Files.Get200pi25lt5m()).Concat(Files.Get400pi100lt9m()))
                .GenerateStream(1.0)
                .AsGoodJetStream()
                .FilterSignal();

            using (var outputHistograms = new FutureTFile("JetMVATraining.root"))
            {
                // Plot the pt spectra before flattening.
                background = background
                    .FlattenPtSpectra(outputHistograms, "background");
                signal = signal
                    .FlattenPtSpectra(outputHistograms, "signal");

                // Finally, plots of all the training input variables.
                var backgroundTrainingData = background
                    .AsTrainingTree()
                    .PlotTrainingVariables(outputHistograms.mkdir("background"), "training_background");

                var signalTrainingData = signal
                    .AsTrainingTree()
                    .PlotTrainingVariables(outputHistograms.mkdir("signal"), "training_signal");

                // Now, do the training.

                var training = signalTrainingData
                    .AsSignal()
                    .Background(backgroundTrainingData)
                    .IgnoreVariables(t => t.JetEta);

                var m1 = training.AddMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA.kBDT, "BDT");

                var trainingResult = training.Train("JetMVATraining");

                // And, finally, generate some efficiency plots.

                var cuts = new List<CutInfo>()
                {
                    new CutInfo() {Title="Run1", Cut = js => js.JetInfo.Jet.logRatio > 1.2 && !js.JetInfo.Tracks.Any() },
                };

                // Calculate the background efficiency for the standard Run 1 cut.
                var standardBackgroundEff = background
                    .CalcualteEfficiency(cuts[0].Cut, js => js.Weight);
                FutureWriteLine(() => $"The background efficiency: {standardBackgroundEff.Value}");

                // Next, calculate the cut for the MVA with that background efficiency
                var nncut = backgroundTrainingData
                    .FindNNCut(1.0 - standardBackgroundEff.Value, outputHistograms.mkdir("jet_mva_background"), trainingResult.GenerateWeightFile(m1));
                FutureWriteLine(() => $"The MVA cut for background efficiency of {standardBackgroundEff.Value} is MVA > {nncut}.");

                cuts.Add(new CutInfo() {
                    Title = "BDT",
                    Cut = js => TrainingUtils.CalculateMVA.Invoke(TrainingUtils.TrainingTreeConverter.Invoke(js), trainingResult.GenerateWeightFile(m1)) > nncut
                });

                // Now dump the signal efficiency for all those cuts we've built.
                foreach (var c in cuts)
                {
                    var eff = GenerateEfficiencyPlots(outputHistograms.mkdir(c.Title), c.Cut, signal);
                    FutureWriteLine(() => $"The signal efficiency for {c.Title}: {eff.Value}.");
                }

                // Done. Dump all output.
                Console.Out.DumpFutureLines();
            }

        }

        /// <summary>
        /// Plot specs to hold onto what we want to plot and weights.
        /// </summary>
        public static IPlotSpec<JetStream> JetStreamPtVsLXYPlot = null;
        public static IPlotSpec<JetStream> JetStreamPtPlot = null;
        public static IPlotSpec<JetStream> JetStreamEtaPlot = null;
        public static IPlotSpec<JetStream> JetStreamLxyPlot = null;

        /// <summary>
        /// List of the 1D plots we want to put out there.
        /// </summary>
        private static List<IPlotSpec<JetStream>> _toPlot = null;

        /// <summary>
        /// Generate the required efficiency plots
        /// </summary>
        /// <param name="outh"></param>
        /// <param name="cut"></param>
        private static IFutureValue<double> GenerateEfficiencyPlots(FutureTDirectory outh, Expression<Func<JetStream, bool>> cut, IQueryable<JetStream> source)
        {
            // Initialize everything the plotters and lists of plots. Sadly, order is important here.
            if (JetStreamPtVsLXYPlot == null)
            {
                JetStreamPtVsLXYPlot = JetPtVsLXYPlot.FromType<recoTreeJets, JetStream>(js => js.JetInfo.Jet, weight: js => js.Weight);
                JetStreamPtPlot = JetPtPlot.FromType<recoTreeJets, JetStream>(js => js.JetInfo.Jet, weight: js => js.Weight);
                JetStreamEtaPlot = JetEtaPlot.FromType<recoTreeJets, JetStream>(js => js.JetInfo.Jet, weight: js => js.Weight);
                JetStreamLxyPlot = JetLxyPlot.FromType<recoTreeJets, JetStream>(js => js.JetInfo.Jet, weight: js => js.Weight);

                _toPlot = new List<IPlotSpec<JetStream>>()
                {
                    JetStreamPtPlot,
                    JetStreamEtaPlot,
                    JetStreamLxyPlot,
                    JetStreamPtVsLXYPlot,
                };
            }

            // Calculate the overall efficiency of this guy.
            var eff = source.CalcualteEfficiency(cut, js => js.Weight);

            // Next, lets do the 1D plots
            _toPlot
                .ForEach(i =>
                {
                    var denominator = source
                        .FuturePlot(i, "denominator");
                    var numerator = source
                        .Where(r => cut.Invoke(r))
                        .FuturePlot(i, "efficiency");

                    (from n in numerator from d in denominator select DivideHistogram(n, d))
                        .Save(outh);
                });

            return eff;
        }

        /// <summary>
        /// A simple divide. returns the numerator, which is what is divided by!
        /// </summary>
        /// <param name="n"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static NTH1 DivideHistogram(NTH1 n, NTH1 d)
        {
            n.Divide(d);
            return n;
        }

        private class CutInfo
        {
            /// <summary>
            /// The actual cut
            /// </summary>
            public Expression<Func<JetStream, bool>> Cut;

            /// <summary>
            /// What we should be calling this thing
            /// </summary>
            public string Title;


        }
    }
}
