using CalRatioTMVAUtilities;
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
using static libDataAccess.CutConstants;
using static libDataAccess.PlotSpecifications;
using static libDataAccess.Utils.CommandLineUtils;
using static libDataAccess.Utils.FutureConsole;
using static libDataAccess.Utils.SampleUtils;
using static LINQToTreeHelpers.PlottingUtils;

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
    }

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
    /// Possible variables for training dataset
    /// </summary>
    public enum TrainingVariableSet
    {
        Default5pT,
        Default5ET,
        DefaultAllpT,
        DefaultAllET,
        None
    }

    /// <summary>
    /// All possible training variables
    /// </summary>
    public enum TrainingVariables
    {
        JetPt,
        CalRatio,
        JetEta,
        NTracks,
        SumPtOfAllTracks,
        MaxTrackPt,
        JetET,
        JetWidth,
        JetTrackDR,
        EnergyDensity,
        HadronicLayer1Fraction,
        JetLat,
        JetLong,
        FirstClusterRadius,
        ShowerCenter
    }

    class Program
    {
        /// <summary>
        /// Run the training for the MVA. This is run in a library,
        /// so basically behind-this-guys-back. But it provides an easy single item to run.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Parse command line arguments
            var options = CommandLineUtils.ParseOptions<Options>(args);

            // Get the signal samples to use for testing and training.
            var signalSources = SampleMetaData.AllSamplesWithTag("mc15c", "signal", "train", "hss")
                .Select(info => Tuple.Create(info.NickName, Files.GetSampleAsMetaData(info, false)))
                .ToArray();

            if (signalSources.Length == 0)
            {
                throw new ArgumentException("No signal sources for training on!");
            }

            var signalUnfiltered = signalSources
                .Aggregate((IQueryable<Files.MetaData>)null, (s, add) => s == null ? add.Item2 : s.Concat(add.Item2))
                .AsGoodJetStream();

            var signalInCalOnly = signalUnfiltered
                .FilterSignal();

            var signalTestSources = SampleMetaData.AllSamplesWithTag("mc15c", "signal", "hss")
                .Select(info => Tuple.Create(info.NickName, Files.GetSampleAsMetaData(info)));

            // Get the background samples to use for testing and training
            var backgroundTrainingTree = BuildBackgroundTrainingTreeDataSource(options);

            // Get the beam-halo samples to use for testing and training
            var data15 = SampleMetaData.AllSamplesWithTag("data15")
                .Take(options.UseFullDataset ? 10000 : 1)
                .SamplesAsSingleQueriable()
                .AsBeamHaloStream(DataEpoc.data15)
                .AsGoodJetStream();

            var data16 = SampleMetaData.AllSamplesWithTag("data16")
                .Take(options.UseFullDataset ? 10000 : 1)
                .SamplesAsSingleQueriable()
                .AsBeamHaloStream(DataEpoc.data16)
                .AsGoodJetStream();

            // The file we will use to dump everything about this training.
            using (var outputHistograms = new FutureTFile("JetMVATraining.root"))
            {
                // Do flattening if requested
                Expression<Func<TrainingTree, double>> toMakeFlat = null;
                switch (options.FlattenBy)
                {
                    case TrainingSpectraFlatteningPossibilities.JetPt:
                        toMakeFlat = t => t.JetPt;
                        FutureWriteLine("Reweighting to flatten as a function of JetPt");
                        break;
                    case TrainingSpectraFlatteningPossibilities.JetET:
                        toMakeFlat = t => t.JetET;
                        FutureWriteLine("Reweighting to flatten as a function of JetEt");
                        break;
                    case TrainingSpectraFlatteningPossibilities.None:
                        FutureWriteLine("Not reweighting at all before training.");
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported flattening request.");
                }
                var flatBackgroundTrainingData = toMakeFlat == null
                    ? backgroundTrainingTree
                    : backgroundTrainingTree
                        .FlattenBySpectra(toMakeFlat, outputHistograms, "background");
                var flatSignalTrainingData = toMakeFlat == null
                    ? signalInCalOnly
                        .AsTrainingTree()
                    : signalInCalOnly
                        .AsTrainingTree()
                        .FlattenBySpectra(toMakeFlat, outputHistograms, "signal");

                // Finally, plots of all the training input variables.
                flatBackgroundTrainingData
                    .PlotTrainingVariables(outputHistograms.mkdir("background"), "training_background");

                flatSignalTrainingData
                    .PlotTrainingVariables(outputHistograms.mkdir("signal"), "training_signal");

                // Get the list of variables we want to use
                var varList = GetTrainingVariables(options);

                // Setup the training
                var training = flatSignalTrainingData
                    .AsSignal(isTrainingEvent: e => !(e.EventNumber % 3 == 1))
                    .Background(flatBackgroundTrainingData, isTrainingEvent: e => !(e.EventNumber % 3 == 1))
                    .UseVariables(varList);

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
                var trainingResult = training.Train("JetMVATraining");

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
                    jobNameBuilder.Append(v.Substring(0, v.Length-2));
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
                        .FilterNonTrainingEvents()
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

                    // And write out a text file that contains the information needed to use this cut.
                    var outf = File.CreateText($"{jobName}_{m.Name}-Info.txt");
                    try
                    {
                        outf.WriteLine($"Using the MVA '{m.Name}' trained in job '{trainingResult.JobName}'");
                        outf.WriteLine();
                        outf.WriteLine($"TMVAReader Weight File: {jobName}_{m.Name}.weights.xml");
                        outf.WriteLine($"  MVAResultValue > {nncut} gives a total background fraction of {standardBackgroundEff.Value}");
                        outf.WriteLine();
                        m.DumpUsageInfo(outf);
                    } finally
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
        /// Return true if we are using JetPt as part of our list of variables.
        /// </summary>
        /// <returns></returns>
        private static bool UsingJetPt(Options opt)
        {
            return GetListOfVariablesToUse(opt)
                .Where(v => v == TrainingVariables.JetPt)
                .Any();
        }

        /// <summary>
        /// Return a list of the training variables that we get by looking at command line options.
        /// </summary>
        /// <returns></returns>
        private static Expression<Func<TrainingTree, double>>[] GetTrainingVariables(Options opt)
        {
            return GetListOfVariablesToUse(opt)
                .Select(v => DictionaryPairForVariable(v))
                .ToArray();
        }

        /// <summary>
        /// Turn a particular type into an expression.
        /// </summary>
        /// <param name="jetPt"></param>
        /// <returns></returns>
        private static Expression<Func<TrainingTree, double>> DictionaryPairForVariable(TrainingVariables varName)
        {
            switch (varName)
            {
                case TrainingVariables.JetPt:
                    return t => t.JetPt;

                case TrainingVariables.CalRatio:
                    return t => t.CalRatio;

                case TrainingVariables.JetEta:
                    return t => t.JetEta;

                case TrainingVariables.NTracks:
                    return t => t.NTracks;

                case TrainingVariables.SumPtOfAllTracks:
                    return t => t.SumPtOfAllTracks;

                case TrainingVariables.MaxTrackPt:
                    return t => t.MaxTrackPt;

                case TrainingVariables.JetET:
                    return t => t.JetET;

                case TrainingVariables.JetWidth:
                    return t => t.JetWidth;

                case TrainingVariables.JetTrackDR:
                    return t => t.JetDRTo2GeVTrack;

                case TrainingVariables.EnergyDensity:
                    return t => t.EnergyDensity;

                case TrainingVariables.HadronicLayer1Fraction:
                    return t => t.HadronicLayer1Fraction;

                case TrainingVariables.JetLat:
                    return t => t.JetLat;

                case TrainingVariables.JetLong:
                    return t => t.JetLong;

                case TrainingVariables.FirstClusterRadius:
                    return t => t.FirstClusterRadius;

                case TrainingVariables.ShowerCenter:
                    return t => t.ShowerCenter;

                default:
                    throw new NotImplementedException($"Unknown variable requested: {varName.ToString()}");
            }
        }

        /// <summary>
        /// Generate the training data source.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Take the same fraction of events from each source.
        /// </remarks>
        private static IQueryable<TrainingTree> BuildBackgroundTrainingTreeDataSource(Options opt)
        {
            // Get the number of events in each source.
            var backgroundSources = CommandLineUtils.GetRequestedBackgroundSourceList();
            var backgroundEventsWithCounts = backgroundSources
                .Select(b => b.Item2.AsGoodJetStream().AsTrainingTree())
                .Select(b => Tuple.Create(b.Count(), b))
                .ToArray();

            // The fraction of weight we want from each source we will take.
            var sourceFraction = ((double)opt.EventsToUseForTrainingAndTesting) / backgroundEventsWithCounts.Select(e => e.Item1).Sum();
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
                JetStreamLxyPlot = JetLxyPlot.FromType<recoTreeJets, JetStream>(js => js.JetInfo.Jet, weight: js => js.Weight);

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

        /// <summary>
        /// Return a list of all variables that we are using.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TrainingVariables> GetListOfVariablesToUse(Options opt)
        {
            var result = new HashSet<TrainingVariables>();

            // First take care of the sets
            switch (opt.TrainingVariableSet)
            {
                case TrainingVariableSet.Default5pT:
                    result.Add(TrainingVariables.JetPt);
                    result.Add(TrainingVariables.CalRatio);
                    result.Add(TrainingVariables.NTracks);
                    result.Add(TrainingVariables.SumPtOfAllTracks);
                    result.Add(TrainingVariables.MaxTrackPt);
                    break;

                case TrainingVariableSet.Default5ET:
                    result.Add(TrainingVariables.JetET);
                    result.Add(TrainingVariables.CalRatio);
                    result.Add(TrainingVariables.NTracks);
                    result.Add(TrainingVariables.SumPtOfAllTracks);
                    result.Add(TrainingVariables.MaxTrackPt);
                    break;

                case TrainingVariableSet.DefaultAllpT:
                    result.Add(TrainingVariables.JetPt);
                    result.Add(TrainingVariables.CalRatio);
                    result.Add(TrainingVariables.NTracks);
                    result.Add(TrainingVariables.SumPtOfAllTracks);
                    result.Add(TrainingVariables.MaxTrackPt);
                    result.Add(TrainingVariables.JetWidth);
                    result.Add(TrainingVariables.JetTrackDR);
                    result.Add(TrainingVariables.EnergyDensity);
                    result.Add(TrainingVariables.HadronicLayer1Fraction);
                    result.Add(TrainingVariables.JetLat);
                    result.Add(TrainingVariables.JetLong);
                    result.Add(TrainingVariables.FirstClusterRadius);
                    result.Add(TrainingVariables.ShowerCenter);
                    break;

                case TrainingVariableSet.DefaultAllET:
                    result.Add(TrainingVariables.JetET);
                    result.Add(TrainingVariables.CalRatio);
                    result.Add(TrainingVariables.NTracks);
                    result.Add(TrainingVariables.SumPtOfAllTracks);
                    result.Add(TrainingVariables.MaxTrackPt);
                    result.Add(TrainingVariables.JetWidth);
                    result.Add(TrainingVariables.JetTrackDR);
                    result.Add(TrainingVariables.EnergyDensity);
                    result.Add(TrainingVariables.HadronicLayer1Fraction);
                    result.Add(TrainingVariables.JetLat);
                    result.Add(TrainingVariables.JetLong);
                    result.Add(TrainingVariables.FirstClusterRadius);
                    result.Add(TrainingVariables.ShowerCenter);
                    break;

                case TrainingVariableSet.None:
                    break;

                default:
                    throw new NotImplementedException();
            }

            // Additional variables
            foreach (var v in opt.AddVariable)
            {
                result.Add(v);
            }

            // Remove any that we want to drop
            foreach (var v in opt.DropVariable)
            {
                result.Remove(v);
            }

            return result;
        }
    }
}
