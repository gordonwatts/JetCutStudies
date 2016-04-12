﻿using DiVertAnalysis;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LinqToTTreeInterfacesLib;
using LINQToTTreeLib;
using ROOTNET.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TMVAUtilities;
using static libDataAccess.Utils.SampleUtils;
using static libDataAccess.PlotSpecifications;
using static libDataAccess.Utils.FutureConsole;
using static LINQToTreeHelpers.PlottingUtils;

namespace JetMVATraining
{
    class Program
    {
        /// <summary>
        /// Total number of background events to use. It will be split equally amongst all JZ samples.
        /// </summary>
        const int NumberOfBackgroundEvents = 500000;

        /// <summary>
        /// Run the training for the MVA. This is run in a library,
        /// so basically behind-this-guys-back. But it provides an easy single item to run.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Parse command line arguments
            CommandLineUtils.Parse(args);

            // Get the background and signal data trees for use (and training.
            var backgroundTrainingTree = BuildBackgroundTrainingTreeDataSource();

            var signalSources = new List<Tuple<string, IQueryable<Files.MetaData>>>() {
                Tuple.Create("600pi150lt9m", Files.Get600pi150lt9m().GenerateStream(1.0)),
                Tuple.Create("400pi100lt9m", Files.Get400pi100lt9m().GenerateStream(1.0)),
                Tuple.Create("200pi25lt5m", Files.Get200pi25lt5m().GenerateStream(1.0)),
            };

            var signalUnfiltered = signalSources
                .Aggregate((IQueryable<Files.MetaData>)null, (s, add) => s == null ? add.Item2 : s.Concat(add.Item2))
                .AsGoodJetStream();

            var signalInCalOnly = signalUnfiltered
                .FilterSignal();

            // The file we will use to dump everything about this training.
            using (var outputHistograms = new FutureTFile("JetMVATraining.root"))
            {
                // Create the training data and flatten the pT spectra.
                var flatBackgroundTrainingData = backgroundTrainingTree
                    .FlattenPtSpectra(outputHistograms, "background")
                    ;
                var flatSignalTrainingData = signalInCalOnly
                    .AsTrainingTree()
                    .FlattenPtSpectra(outputHistograms, "signal")
                    ;

                // Finally, plots of all the training input variables.
                flatBackgroundTrainingData
                    .PlotTrainingVariables(outputHistograms.mkdir("background"), "training_background");

                flatSignalTrainingData
                    .PlotTrainingVariables(outputHistograms.mkdir("signal"), "training_signal");

                // Now, do the training.

                var training = flatSignalTrainingData
                    .AsSignal()
                    .Background(flatBackgroundTrainingData)
                    .IgnoreVariables(t => t.JetEta);

                // Build options (like what we might like to transform.
                var m1 = training.AddMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA.kBDT, "BDT")
                    .Option("MaxDepth", CommandLineUtils.MaxBDTDepth.ToString())
                    .Option("MinNodeSize", CommandLineUtils.BDTLeafMinFraction.ToString())
                    .Option("nCuts", "200")
                    ;

                if (!string.IsNullOrWhiteSpace(CommandLineUtils.TrainingVariableTransform))
                {
                    m1.Option("VarTransform", CommandLineUtils.TrainingVariableTransform);
                }

                var methods = new List<Method<TrainingTree>>();
                methods.Add(m1);

                // Do the training
                var trainingResult = training.Train("JetMVATraining");

                // Copy to a common filename. We do this only because it makes
                // the Jenkins artifacts to pick up only what we are producing this round.
                trainingResult.CopyToJobName();

                // And, finally, generate some efficiency plots.
                // First, get the list of cuts we are going to use. Start with the boring Run 1.
                var cuts = new List<CutInfo>()
                {
                    new CutInfo() {
                        Title ="Run1",
                        Cut = js => js.JetInfo.Jet.logRatio > 1.2 && !js.JetInfo.Tracks.Any(),
                        CutValue = js => js.JetInfo.Jet.logRatio > 1.2 && !js.JetInfo.Tracks.Any() ? 1.0 : 0.0
                    },
                };

                // Now, for each of the trained methods, we need to do the same thing.
                var fullBackgroundSample = Files.GetAllJetSamples()
                    .AsGoodJetStream();
                var standardBackgroundEff = fullBackgroundSample
                    .CalcualteEfficiency(cuts[0].Cut, js => js.Weight);
                FutureWriteLine(() => $"The background efficiency for Run 1 cuts: {standardBackgroundEff.Value}");
                foreach (var m in methods)
                {
                    // Calculate where we have to place the cut in order to get the same over-all background efficiency.
                    var nncut = fullBackgroundSample
                        .AsTrainingTree()
                        .FindNNCut(1.0 - standardBackgroundEff.Value, outputHistograms.mkdir("jet_mva_background"), m);
                    FutureWriteLine(() => $"The MVA cut for background efficiency of {standardBackgroundEff.Value} is {m.Name} > {nncut}.");

                    // Now build a new cut object and add it into the cut list.
                    var cBDT = m1.GetMVAValue();
                    cuts.Add(new CutInfo()
                    {
                        Title = m.Name,
                        Cut = js => cBDT.Invoke(TrainingUtils.TrainingTreeConverter.Invoke(js)) > nncut,
                        CutValue = js => cBDT.Invoke(TrainingUtils.TrainingTreeConverter.Invoke(js))
                    });
                }

                // Now dump the signal efficiency for all those cuts we've built.
                foreach (var c in cuts)
                {
                    var cutDir = outputHistograms.mkdir(c.Title);
                    foreach (var s in signalSources)
                    {
                        var sEvents = s.Item2
                            .AsGoodJetStream()
                            .FilterLLPNear();
                        var leff = GenerateEfficiencyPlots(cutDir.mkdir(s.Item1), c.Cut, c.CutValue, sEvents);
                        FutureWriteLine(() => $"The signal efficiency for {c.Title} {s.Item1}: {leff.Value}.");
                    }
                    var eff = GenerateEfficiencyPlots(cutDir.mkdir("AllSignal"), c.Cut, c.CutValue, signalInCalOnly);
                    FutureWriteLine(() => $"The signal efficiency for {c.Title} TrainingSignalEvents: {eff.Value}.");
                }

                // Done. Dump all output.
                Console.Out.DumpFutureLines();
            }

        }

        /// <summary>
        /// Generate the training data source.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Take the same fraction of events from each source.
        /// </remarks>
        private static IQueryable<TrainingTree> BuildBackgroundTrainingTreeDataSource()
        {
            // Get the number of events in each source.
            var backgroundSources = CommandLineUtils.GetRequestedBackgroundSourceList();
            var backgroundEventsWithCounts = backgroundSources
                .Select(b => b.Item2.AsGoodJetStream().AsTrainingTree())
                .Select(b => Tuple.Create(b.Count(), b))
                .ToArray();

            // The fraction of weight we want from each source we will take.
            var sourceFraction = ((double)NumberOfBackgroundEvents) / backgroundEventsWithCounts.Select(e => e.Item1).Sum();
            sourceFraction = sourceFraction > 1.0 ? 1.0 : sourceFraction;

            // Build a stream of all the backgrounds, stitched together.
            return backgroundEventsWithCounts
                .Select(e => e.Item2.Take((int)(e.Item1 * sourceFraction)))
                .Aggregate((IQueryable<TrainingTree>)null, (s, add) => s == null ? add : s.Concat(add));
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
        private static IFutureValue<double> GenerateEfficiencyPlots(FutureTDirectory outh, Expression<Func<JetStream, bool>> cut, Expression<Func<JetStream, double>> cVal, IQueryable<JetStream> source)
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

            // And the MVA output we cut for both (no efficiency required).
            source
                .Select(j => Tuple.Create(cVal.Invoke(j), j.Weight))
                .FuturePlot(TrainingEventWeight.NameFormat, TrainingEventWeight.TitleFormat, TrainingEventWeight, "ForJetsWithLLPNear")
                .Save(outh);

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
            /// What is the value of the MVA or cut?
            /// </summary>
            public Expression<Func<JetStream, double>> CutValue;

            /// <summary>
            /// What we should be calling this thing
            /// </summary>
            public string Title;


        }
    }
}
