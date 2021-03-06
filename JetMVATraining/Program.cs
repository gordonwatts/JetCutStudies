﻿using CalRatioTMVAUtilities;
using CommandLine;
using DiVertAnalysis;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LinqToTTreeInterfacesLib;
using LINQToTTreeLib;
using ROOTNET.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TMVAUtilities;
using static CalRatioTMVAUtilities.PtReweightUtils;
using static CalRatioTMVAUtilities.TrainingVariableUtils;
using static libDataAccess.CutConstants;
using static libDataAccess.PlotSpecifications;
using static libDataAccess.Utils.CommandLineUtils;
using static libDataAccess.Utils.FutureConsole;
using static libDataAccess.Utils.SampleUtils;
using static LINQToTreeHelpers.PlottingUtils;
using static CalRatioTMVAUtilities.BackgroundSampleUtils;
using System.Threading.Tasks;

namespace JetMVATraining
{
    /// <summary>
    /// Options for running the Jet training.
    /// </summary>
    public class Options : CommonOptions
    {
        [Option("BDTMaxDepth", Default = 3)]
        public int BDTMaxDepth { get; set; }

        [Option("BDTLeafMinFraction", Default = 5)]
        public double BDTLeafMinFraction { get; set; }

        [Option("TrainingEvents", Default = 500000)]
        public int EventsToUseForTrainingAndTesting { get; set; }

        [Option("VariableTransform", Default = "")]
        public string VariableTransform { get; set; }

        [Option("TrainingVariableSet", Default = TrainingVariableSet.DefaultAllpT)]
        public TrainingVariableSet TrainingVariableSet { get; set; }

        [Option("DropVariable")]
        public IEnumerable<TrainingVariables> DropVariable { get; set; }

        [Option("AddVariable")]
        public IEnumerable<TrainingVariables> AddVariable { get; set; }

        [Option("FlattenBy", Default = TrainingSpectraFlatteningPossibilities.JetPt)]
        public TrainingSpectraFlatteningPossibilities FlattenBy { get; set; }

        [Option("TrainEventsBIB15", HelpText ="How many events from data15 should be used in the training?", Default = 0)]
        public int EventsToUseForTrainingAndTestingBIB15 { get; set; }

        [Option("TrainEventsBIB16", HelpText = "How many events from data16 should be used in the training?", Default = 0)]
        public int EventsToUseForTrainingAndTestingBIB16 { get; set; }

        [Option("SmallTestingMenu", HelpText ="If present, then run on a small number of samples", Default = false)]
        public bool SmallTestingMenu { get; set; }
    }

    class Program
    {
        /// <summary>
        /// Run the training for the MVA. This is run in a library,
        /// so basically behind-this-guys-back. But it provides an easy single item to run.
        /// </summary>
        /// <param name="args"></param>
        static async Task Main(string[] args)
        {
            // Parse command line arguments
            var options = CommandLineUtils.ParseOptions<Options>(args);

            // Get the signal samples to use for testing and training.
            var signalSources = await SampleMetaData.AllSamplesWithTag("mc15c", "signal", "train", "hss")
                .Select(async info => Tuple.Create(info.NickName, await Files.GetSampleAsMetaData(info, false)))
                .WhenAll();

            if (signalSources.Length == 0)
            {
                throw new ArgumentException("No signal sources for training on!");
            }

            var signalUnfiltered = signalSources
                .Aggregate((IQueryable<Files.MetaData>)null, (s, add) => s == null ? add.Item2 : s.Concat(add.Item2))
                .AsGoodJetStream();

            var signalInCalOnly = signalUnfiltered
                .FilterSignal();

            var tags = new string[] { "mc15c", "signal", "hss" }.Add(options.SmallTestingMenu ? "quick_compare" : "compare");
            var signalTestSources = await SampleMetaData.AllSamplesWithTag(tags.ToArray())
                .Select(async info => Tuple.Create(info.NickName, await Files.GetSampleAsMetaData(info)))
                .WhenAll();

            // Get the background samples to use for testing and training
            var backgroundTrainingTree = await BuildBackgroundTrainingTreeDataSource(options.EventsToUseForTrainingAndTesting);

            // Get the beam-halo samples to use for testing and training
            var data15 = (await SampleMetaData.AllSamplesWithTag("data15_new")
                .Take(options.UseFullDataset ? 10000 : 1)
                .SamplesAsSingleQueriable())
                .AsBeamHaloStream(DataEpoc.data15)
                .AsGoodJetStream();

            var data15TrainingAndTesting = data15
                .Take(options.EventsToUseForTrainingAndTestingBIB15);

            var data16 = (await SampleMetaData.AllSamplesWithTag("data16")
                .Take(options.UseFullDataset ? 10000 : 1)
                .SamplesAsSingleQueriable())
                .AsBeamHaloStream(DataEpoc.data16)
                .AsGoodJetStream();

            var data16TrainingAndTesting = data16
                .Take(options.EventsToUseForTrainingAndTestingBIB16);

            // The file we will use to dump everything about this training.
            using (var outputHistograms = new FutureTFile("JetMVATraining.root"))
            {
                // Flatten everything as needed.
                var toMakeFlat = BuildFlatteningExpression(options.FlattenBy);
                var flatBackgroundTrainingData = FlattenTrainingTree(backgroundTrainingTree, outputHistograms, toMakeFlat);
                var flatSignalTrainingData = FlattenTrainingTree(signalInCalOnly.AsTrainingTree(), outputHistograms, toMakeFlat);
                var flatData15 = FlattenTrainingTree(signalInCalOnly.AsTrainingTree(), outputHistograms, toMakeFlat);
                var flatData16 = FlattenTrainingTree(data16TrainingAndTesting.AsTrainingTree(), outputHistograms, toMakeFlat);

                // Finally, plots of all the training input variables.
                flatBackgroundTrainingData
                    .PlotTrainingVariables(outputHistograms.mkdir("background"), "training_background");

                flatSignalTrainingData
                    .PlotTrainingVariables(outputHistograms.mkdir("signal"), "training_signal");

                flatData15
                    .PlotTrainingVariables(outputHistograms.mkdir("data15"), "training_bib15");

                flatData16
                    .PlotTrainingVariables(outputHistograms.mkdir("data16"), "training_bib16");

                // Get the list of variables we want to use
                var varList = GetTrainingVariables(options.TrainingVariableSet, options.AddVariable.ToArray(), options.DropVariable.ToArray());

                // Setup the training
                var training = flatSignalTrainingData
                    .AsSignal(isTrainingEvent: e => !(e.EventNumber % 3 == 1))
                    .Background(flatBackgroundTrainingData, isTrainingEvent: e => !(e.EventNumber % 3 == 1))
                    .UseVariables(varList);

                if (options.EventsToUseForTrainingAndTestingBIB15 > 0)
                {
                    training.
                        Background(flatData15, isTrainingEvent: e => !(e.EventNumber % 3 == 1));
                }
                if (options.EventsToUseForTrainingAndTestingBIB16 > 0)
                {
                    training.
                        Background(flatData16, isTrainingEvent: e => !(e.EventNumber % 3 == 1));
                }

                // Build options (like what we might like to transform.
                var m1 = training.AddMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA.kBDT, "BDT")
                    .Option("MaxDepth", options.BDTMaxDepth.ToString())
                    .Option("MinNodeSize", options.BDTLeafMinFraction.ToString())
                    .Option("nCuts", "200")
                    ;

                if (!string.IsNullOrWhiteSpace(options.VariableTransform))
                {
                    m1.Option("VarTransform", options.VariableTransform);
                }

                var methods = new List<Method<TrainingTree>>();
                methods.Add(m1);

                // Do the training
                var trainingResult = await training.Train("JetMVATraining");

                // Build a job name.
                var jobNameBuilder = new StringBuilder();
                jobNameBuilder.Append($"Jet.MVATraining-");
                bool first = true;
                foreach (var v in training.UsedVariables())
                {
                    if (!first)
                    {
                        jobNameBuilder.Append(".");
                    }
                    first = false;
                    jobNameBuilder.Append(v);
                }
                var jobName = jobNameBuilder.ToString();

                // Copy to a common filename. We do this only because it makes
                // the Jenkins artifacts to pick up only what we are producing this round.
                trainingResult.CopyToJobName(jobName);

                // And, finally, generate some efficiency plots.
                // First, get the list of cuts we are going to use. Start with the boring Run 1.
                var cuts = new List<CutInfo>()
                {
                    new CutInfo() {
                        Title ="Run1",
                        Cut = js => js.JetInfo.Jet.logRatio > LogRatioCut && !js.JetInfo.Tracks.Any(),
                        CutValue = js => js.JetInfo.Jet.logRatio > LogRatioCut && !js.JetInfo.Tracks.Any() ? 1.0 : 0.0
                    },
                };

                // Now, for each of the trained methods, we need to do the same thing.
                var fullBackgroundSample = (await Files.GetAllJetSamples())
                    .AsGoodJetStream();
                var standardBackgroundEff = fullBackgroundSample
                    .CalcualteEfficiency(cuts[0].Cut, js => js.Weight);
                FutureWriteLine(() => $"The background efficiency for Run 1 cuts: {standardBackgroundEff.Value}");
                foreach (var m in methods)
                {
                    // Calculate where we have to place the cut in order to get the same over-all background efficiency.
                    var nncut = fullBackgroundSample
                        .AsTrainingTree()
                        .FilterNonTrainingEvents()
                        .FindNNCut(1.0 - standardBackgroundEff.Value, outputHistograms.mkdir("jet_mva_background"), m);
                    FutureWriteLine(() => $"The MVA cut for background efficiency of {standardBackgroundEff.Value} is {m.Name} > {nncut}.");

                    // Now build a new cut object and add it into the cut list.
                    var cBDT = m1.GetMVAValue();
                    cuts.Add(new CutInfo()
                    {
                        Title = m.Name,
                        Cut = js => cBDT.Invoke(TrainingUtils.TrainingTreeConverter.Invoke(js)) > nncut.Value,
                        CutValue = js => cBDT.Invoke(TrainingUtils.TrainingTreeConverter.Invoke(js))
                    });

                    // And write out a text file that contains the information needed to use this cut.
                    var outf = File.CreateText(PathUtils.ControlFilename(jobName, new DirectoryInfo("."), n => $"{n}_{m.Name}-Info.txt"));
                    try
                    {
                        outf.WriteLine($"Using the MVA '{m.Name}' trained in job '{trainingResult.JobName}'");
                        outf.WriteLine();
                        outf.WriteLine($"TMVAReader Weight File: {jobName}_{m.Name}.weights.xml");
                        outf.WriteLine($"  MVAResultValue > {nncut.Value} gives a total background fraction of {standardBackgroundEff.Value}");
                        outf.WriteLine();
                        m.DumpUsageInfo(outf);
                    }
                    finally
                    {
                        outf.Close();
                    }
                }

                // Now dump the signal efficiency for all those cuts we've built.
                foreach (var c in cuts)
                {
                    var cutDir = outputHistograms.mkdir(c.Title);
                    foreach (var s in signalTestSources)
                    {
                        var sEvents = s.Item2
                            .AsGoodJetStream()
                            .FilterNonTrainingEvents()
                            .FilterLLPNear();
                        var leff = GenerateEfficiencyPlots(cutDir.mkdir(s.Item1), c.Cut, c.CutValue, sEvents);
                        FutureWriteLine(() => $"The signal efficiency for {c.Title} {s.Item1} {leff.Value}");
                    }

                    // Signal in the calorimeter only
                    var effTest = GenerateEfficiencyPlots(cutDir.mkdir("AllSignalNonTraining"), c.Cut, c.CutValue, signalInCalOnly.FilterNonTrainingEvents());
                    var effTrain = GenerateEfficiencyPlots(cutDir.mkdir("AllSignalTraining"), c.Cut, c.CutValue, signalInCalOnly.FilterTrainingEvents());
                    FutureWriteLine(() => $"The signal efficiency for {c.Title} TestingSignalEvents {effTest.Value}");
                    FutureWriteLine(() => $"The signal efficiency for {c.Title} TrainingSignalEvents {effTrain.Value}");

                    // And beam halo
                    var effData15 = GenerateEfficiencyPlots(cutDir.mkdir("beamhalo15"), c.Cut, c.CutValue, data15);
                    var effData16 = GenerateEfficiencyPlots(cutDir.mkdir("beamhalo16"), c.Cut, c.CutValue, data16);
                    FutureWriteLine(() => $"The signal efficiency for {c.Title} data15 {effData15.Value}");
                    FutureWriteLine(() => $"The signal efficiency for {c.Title} data16 {effData16.Value}");
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
        public static IPlotSpec<JetStream> JetStreamETPlot = null;
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
                JetStreamETPlot = JetETPlot.FromType<recoTreeJets, JetStream>(js => js.JetInfo.Jet, weight: js => js.Weight);
                JetStreamEtaPlot = JetEtaPlot.FromType<recoTreeJets, JetStream>(js => js.JetInfo.Jet, weight: js => js.Weight);
                JetStreamLxyPlot = JetLLPLxyPlot.FromType<recoTreeJets, JetStream>(js => js.JetInfo.Jet, weight: js => js.Weight);

                _toPlot = new List<IPlotSpec<JetStream>>()
                {
                    JetStreamPtPlot,
                    JetStreamETPlot,
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
