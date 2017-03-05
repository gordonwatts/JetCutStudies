using CalRatioTMVAUtilities;
using CommandLine;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TMVAUtilities;
using static CalRatioTMVAUtilities.BackgroundSampleUtils;
using static CalRatioTMVAUtilities.PtReweightUtils;
using static CalRatioTMVAUtilities.TrainingVariableUtils;
using static libDataAccess.PlotSpecifications;
using static libDataAccess.Utils.CommandLineUtils;
using static libDataAccess.Utils.SampleUtils;
using static libDataAccess.Utils.FutureConsole;

namespace JetMVAClassifierTraining
{
    class Program
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

            [Option("SmallTestingMenu", HelpText = "If present, then run on a small number of samples", Default = false)]
            public bool SmallTestingMenu { get; set; }

            [Option("TrainEventsBIB16", HelpText = "How many events from data16 should be used in the training for bib16 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB16 { get; set; }

            [Option("TrainEventsBIB15", HelpText = "How many events from data16 should be used in the training for bib15 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB15 { get; set; }

            [Option("PrecisionValue", HelpText ="The fraction of events in each sample to use when calculating the training precision", Default = 0.90)]
            public double PrecisionValue { get; set; }

            [Option("pTCut", HelpText ="The pT cut for jets in GeV. Defaults to 40.", Default = 40.0)]
            public double pTCut { get; set; }
        }

        static void Main(string[] args)
        {
            // Parse command line arguments
            var options = CommandLineUtils.ParseOptions<Options>(args);

            // Class: LLP
            var signalSources = SampleMetaData.AllSamplesWithTag("mc15c", "signal", "train", "hss")
                .Take(options.UseFullDataset ? 10000 : 2)
                .Select(info => Tuple.Create(info.NickName, Files.GetSampleAsMetaData(info, false)))
                .ToArray();

            if (signalSources.Length == 0)
            {
                throw new ArgumentException("No signal sources for training on!");
            }

            var signalUnfiltered = signalSources
                .Aggregate((IQueryable<Files.MetaData>)null, (s, add) => s == null ? add.Item2 : s.Concat(add.Item2))
                .AsGoodJetStream(options.pTCut);

            var signalInCalOnly = signalUnfiltered
                .FilterSignal();

            // Class: Multijet
            var backgroundTrainingTree = BuildBackgroundTrainingTreeDataSource(options.EventsToUseForTrainingAndTesting, options.pTCut, !options.UseFullDataset);

            // Class: BIB
            var data15TrainingAndTesting = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB15 < 0 ? (options.UseFullDataset ? -1 : 25000) : options.EventsToUseForTrainingAndTestingBIB16, DataEpoc.data15, options.pTCut);
            var data16TrainingAndTesting = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB16 < 0 ? (options.UseFullDataset ? -1 : 25000) : options.EventsToUseForTrainingAndTestingBIB15, DataEpoc.data16, options.pTCut);


            // The file we will use to dump everything about this training.
            using (var outputHistograms = new FutureTFile("JetMVAClassifierTraining.root"))
            {
                // Flatten everything as needed.
                var toMakeFlat = BuildFlatteningExpression(options.FlattenBy);
                var flatBackgroundTrainingData = FlattenTrainingTree(backgroundTrainingTree, outputHistograms, toMakeFlat);
                var flatSignalTrainingData = FlattenTrainingTree(signalInCalOnly.AsTrainingTree(), outputHistograms, toMakeFlat);
                var flatData15 = FlattenTrainingTree(data15TrainingAndTesting.AsTrainingTree(), outputHistograms, toMakeFlat);
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
                    .AsClass("hss", isTrainingEvent: e => !(e.EventNumber % 3 == 1), title: "hss")
                    .EventClass(flatBackgroundTrainingData, "Multijet", isTrainingEvent: e => !(e.EventNumber % 3 == 1), title: "multijet")
                    .EventClass(flatData16, "BIB", isTrainingEvent: e => !(e.EventNumber % 3 == 1), title: "bib_16")
                    .EventClass(flatData15, "BIB", isTrainingEvent: e => !(e.EventNumber % 3 == 1), title: "bib_15")
                    .UseVariables(varList);

                // Create the BDT training and configure it.
                // Note: BoostType has to be Grad for multi-class.
                var m1 = training.AddMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA.kBDT, "BDT")
                    .Option("MaxDepth", options.BDTMaxDepth.ToString())
                    .Option("MinNodeSize", options.BDTLeafMinFraction.ToString())
                    .Option("nCuts", "200")
                    .Option("BoostType", "Grad")
                    ;

                if (!string.IsNullOrWhiteSpace(options.VariableTransform))
                {
                    m1.Option("VarTransform", options.VariableTransform);
                }

                var methods = new List<Method<TrainingTree>>();
                methods.Add(m1);

                // Do the training
                var trainingResult = training.Train("JetMVAClassifier");

                // Build a job name.
                var jobNameBuilder = new StringBuilder();
                jobNameBuilder.Append($"JetMVAClass-");
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

                // And write out a text file that contains the information needed to use this cut.
                var outf = File.CreateText(PathUtils.ControlFilename(jobName, new DirectoryInfo("."), n => $"{n}_{m1.Name}-Info.txt"));
                try
                {
                    outf.WriteLine($"Using the MVA '{m1.Name}' trained in job '{trainingResult.JobName}'");
                    outf.WriteLine();
                    outf.WriteLine($"TMVAReader Weight File: {jobName}_{m1.Name}.weights.xml");
                    outf.WriteLine();
                    m1.DumpUsageInfo(outf);
                }
                finally
                {
                    outf.Close();
                }

                // Now, for each sample, generate the weight plots
                var trainingResultDir = outputHistograms.mkdir("Results");
                var tags = new string[] { "mc15c", "signal", "hss" }.Add(options.SmallTestingMenu ? "quick_compare" : "compare");
                var signalTestSources = SampleMetaData.AllSamplesWithTag(tags.ToArray())
                    .Select(info => Tuple.Create(info.NickName, Files.GetSampleAsMetaData(info)));
                var cBDT = m1.GetMVAMulticlassValue();
                foreach (var s in signalTestSources)
                {
                    var sEvents = s.Item2
                        .AsGoodJetStream(options.pTCut)
                        .FilterNonTrainingEvents()
                        .FilterLLPNear()
                        .AsTrainingTree();

                    GenerateEfficiencyPlots(trainingResultDir.mkdir(s.Item1), sEvents, cBDT, new string[] { "hss", "multijet", "bib" });
                }
                GenerateEfficiencyPlots(trainingResultDir.mkdir("data15"), data15TrainingAndTesting.AsTrainingTree(), cBDT, new string[] { "hss", "multijet", "bib" });
                GenerateEfficiencyPlots(trainingResultDir.mkdir("data16"), data16TrainingAndTesting.AsTrainingTree(), cBDT, new string[] { "hss", "multijet", "bib" });
                GenerateEfficiencyPlots(trainingResultDir.mkdir("jz"), backgroundTrainingTree, cBDT, new string[] { "hss", "multijet", "bib" });

                // Calculate the cut value for each output in order to determine the precision.
                // Calculate where we have to place the cut in order to get the same over-all background efficiency.
                var effhistDirectories = outputHistograms.mkdir("prec_calc");
                var nncutSig = signalInCalOnly
                    .AsTrainingTree()
                    .FilterNonTrainingEvents()
                    .FindNNCut(options.PrecisionValue, effhistDirectories, m1, 0, name: "Signal");
                var nncutMultijet = backgroundTrainingTree
                    .FilterNonTrainingEvents()
                    .FindNNCut(options.PrecisionValue, effhistDirectories, m1, 1, name: "Multijet");
                var nncutBiB = data15TrainingAndTesting == null
                    ? 0.5.AsFuture()
                    : data15TrainingAndTesting
                    .AsTrainingTree()
                    .FilterNonTrainingEvents()
                    .FindNNCut(options.PrecisionValue, effhistDirectories, m1, 2, name: "BIB");

                FutureWriteLine(() => $"The MVA cut signal efficiency of {options.PrecisionValue} is {nncutSig.Value}");
                FutureWriteLine(() => $"The MVA cut multijet efficiency of {options.PrecisionValue} is {nncutMultijet.Value}");
                FutureWriteLine(() => $"The MVA cut BIB efficiency of {options.PrecisionValue} is {nncutBiB.Value}");
                var average = from nnSig in nncutSig
                              from nnMul in nncutMultijet
                              from nnBIB in nncutBiB
                              select (nnSig + nnMul + nnBIB) / 3.0;
                FutureWriteLine(() => $"The average MVA cut for {options.PrecisionValue} pass rate is {average.Value}");

                // Next, calc the 90% eff for each sample, and the average
                foreach (var s in signalTestSources)
                {
                    var nnCutTestSignal = s.Item2
                        .AsGoodJetStream(options.pTCut)
                        .AsTrainingTree()
                        .FilterNonTrainingEvents()
                        .FindNNCut(options.PrecisionValue, effhistDirectories, m1, 0, name: s.Item1);
                    FutureWriteLine(() => $"The MVA cut {s.Item1} efficiency of {options.PrecisionValue} is {nnCutTestSignal.Value}");
                    var a = from nnSig in nnCutTestSignal
                            from nnMul in nncutMultijet
                            from nnBIB in nncutBiB
                            select (nnSig + nnMul + nnBIB) / 3.0;
                    FutureWriteLine(() => $"  The average MVA cut for {s.Item1} for {options.PrecisionValue} pass rate is {a.Value}");
                }

                // Done. Dump all output.
                Console.Out.DumpFutureLines();
            }
        }

        /// <summary>
        /// Grab the BIB samles
        /// </summary>
        /// <param name="requestedNumberOfEvents">-1 for everything, or a number of requested</param>
        /// <param name="bib_tag">The tag name we should use to do the lookup</param>
        /// <returns></returns>
        private static IQueryable<JetStream> GetBIBSamples(int requestedNumberOfEvents, DataEpoc epoc, double pTCut)
        {
            // If no events, then we need to just return everything
            if (requestedNumberOfEvents == 0)
            {
                return null;
            }

            // Fetch all the data samples
            var dataSamples = SampleMetaData.AllSamplesWithTag(epoc == DataEpoc.data15 ? "data15" : "data16");

            // If we have a limitation on the number of events, then we need to measure our the # of events.
            int countOfEvents = 0;
            int countOfEventsOneBack = 0;
            dataSamples = dataSamples
                .TakeWhile(s =>
                {
                    if (requestedNumberOfEvents < 0)
                    {
                        return true;
                    }
                    var q = Files.GetSampleAsMetaData(s);
                    countOfEventsOneBack = countOfEvents;
                    countOfEvents += q.AsBeamHaloStream(epoc)
                                        .AsGoodJetStream(pTCut)
                                        .Count();
                    return countOfEvents < requestedNumberOfEvents;
                })
                .ToArray();

            // The following is the tricky part. Now that we have a list of events, it is not likely that we have found a file boundary
            // that matches the number of events. So we will have to do this a little carefully.

            SampleMetaData theLastSample = null;
            IEnumerable<SampleMetaData> allBut = dataSamples;
            if (countOfEvents > 0 && countOfEvents > requestedNumberOfEvents)
            {
                // Take up to the last one.
                allBut = dataSamples.Take(dataSamples.Count() - 1);
                theLastSample = dataSamples.Last();
            }

            var data1 = allBut
                .SamplesAsSingleQueriable()
                .AsBeamHaloStream(epoc)
                .AsGoodJetStream(pTCut);

            var data = theLastSample == null ? data1
                : data1.Concat(Files.GetSampleAsMetaData(theLastSample).AsBeamHaloStream(epoc).AsGoodJetStream(pTCut).Take(requestedNumberOfEvents - countOfEventsOneBack));

            return data;
        }

        /// <summary>
        /// Generate plots for everything
        /// </summary>
        /// <param name="futureTDirectory"></param>
        /// <param name="sEvents"></param>
        /// <param name="cBDT"></param>
        private static void GenerateEfficiencyPlots(FutureTDirectory outh, IQueryable<TrainingTree> source, Expression<Func<TrainingTree, float[]>> cBDT, string[] trainingClassNames)
        {
            if (source == null)
            {
                return;
            }

            var s1 = source
                .Select(j => Tuple.Create(cBDT.Invoke(j), j.Weight));

            foreach (var cinfo in trainingClassNames.Zip(Enumerable.Range(0, trainingClassNames.Length), (name, index) => Tuple.Create(name, index)))
            {
                s1
                    .Select(n => Tuple.Create((double) n.Item1[cinfo.Item2], n.Item2))
                    .FuturePlot(ClassifierEventWeight.NameFormat, ClassifierEventWeight.TitleFormat, ClassifierEventWeight, $"weight_{cinfo.Item1}")
                    .Save(outh);
            }
        }
    }
}
