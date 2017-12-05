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
using static libDataAccess.Utils.BIBSamples;
using LINQToTTreeLib.Files;

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

            [Option("nTrees", HelpText = "How many trees should be trained in the boosting?", Default = 800)]
            public int MaxTreesForTraining { get; set; }

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

            [Option("TrainingEventsJz", Default = -1, HelpText ="Number of events to use in training for JZ sample. -1 means everything. Defaults to 20,000 if UseFullDataset not presen.")]
            public int EventsToUseForJzTraining { get; set; }

            [Option("TrainingEventsSignal", Default = -1, HelpText = "Number of events to use in training for singal sample. -1 means everything. Defaults to 20,000 if UseFullDataset not presen.")]
            public int EventsToUseForSignalTraining { get; set; }

            [Option("TrainEventsBIB16", HelpText = "How many events from data16 should be used in the training for bib16 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB16 { get; set; }

            [Option("TrainEventsBIB15", HelpText = "How many events from data15 should be used in the training for bib15 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB15 { get; set; }

            [Option("PrecisionValue", HelpText ="The fraction of events in each sample to use when calculating the training precision", Default = 0.90)]
            public double PrecisionValue { get; set; }

            [Option("pTCut", HelpText ="The pT cut for jets in GeV.", Default = 40.0)]
            public double pTCut { get; set; }

            [Option("LxyCut", HelpText = "Restrict barrel signal to have a Lxy of at least this value (meters).", Default = 0.0)]
            public double LxyCut { get; set; }

            [Option("LzCut", HelpText = "Restrict endcap signal to have a Lxy of at least this value (meters)", Default = 0.0)]
            public double LzCut { get; set; }
        }

        static void Main(string[] args)
        {
            ConsoleMessageDumper.SetupConsoleMessageDumper();

            // Parse command line arguments
            var options = CommandLineUtils.ParseOptions<Options>(args);

            // Fix up defaults depending on full dataset or not.
            if (!options.UseFullDataset)
            {
                const int SmallNumberOfEvents = 50000;
                options.EventsToUseForJzTraining = options.EventsToUseForJzTraining == -1
                    ? SmallNumberOfEvents
                    : options.EventsToUseForJzTraining;

                options.EventsToUseForSignalTraining = options.EventsToUseForSignalTraining == -1
                    ? SmallNumberOfEvents
                    : options.EventsToUseForSignalTraining;

                options.EventsToUseForTrainingAndTestingBIB15 = options.EventsToUseForTrainingAndTestingBIB15 == -1
                    ? SmallNumberOfEvents
                    : options.EventsToUseForTrainingAndTestingBIB15;

                options.EventsToUseForTrainingAndTestingBIB16 = options.EventsToUseForTrainingAndTestingBIB16 == -1
                    ? SmallNumberOfEvents
                    : options.EventsToUseForTrainingAndTestingBIB16;
            }

            // Class: LLP
            Console.WriteLine("Fetching HSS Sample");
            var signalInCalOnly = SampleMetaData.AllSamplesWithTag("mc15c", "signal", "train", "hss")
                .TakeEventsFromSamlesEvenly(options.EventsToUseForSignalTraining, Files.NFiles*2,
                    mdQueriable => mdQueriable
                                    .AsGoodJetStream(options.pTCut, maxPtCut: TrainingUtils.MaxJetPtForTraining)
                                    .FilterSignal(options.LxyCut*1000.0, options.LzCut*1000.0),
                    weightByCrossSection: false);

            // Class: Multijet
            Console.WriteLine("Fetching JZ Sample");
            var backgroundTrainingTree = BuildBackgroundTrainingTreeDataSource(options.EventsToUseForJzTraining, 
                options.pTCut, Files.NFiles, maxPtCut: TrainingUtils.MaxJetPtForTraining,
                weightByCrossSection: true);

            // Class: BIB
            Console.WriteLine("Fetching BIB15 Sample");
            var data15TrainingAndTesting = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB15, DataEpoc.data15,
                options.pTCut, maxPtCut: TrainingUtils.MaxJetPtForTraining,
                useLessSamples: !options.UseFullDataset);
            Console.WriteLine("Fetching BIB16 Sample");
            var data16TrainingAndTesting = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB16, DataEpoc.data16,
                options.pTCut, maxPtCut: TrainingUtils.MaxJetPtForTraining,
                useLessSamples: !options.UseFullDataset);

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
                    .Option("NTrees", options.MaxTreesForTraining.ToString())
                    .Option("NegWeightTreatment", "IgnoreNegWeightsInTraining")
                    ;

                if (!string.IsNullOrWhiteSpace(options.VariableTransform))
                {
                    m1.Option("VarTransform", options.VariableTransform);
                }

                var methods = new List<Method<TrainingTree>>();
                methods.Add(m1);

                // Do the training
                var trainingResult = training.Train("JetMVAClassifier");

                // Build a job name and coppy everything over to that for easy reference and so we can find the results.
                var jobNameBuilder = new StringBuilder();
                jobNameBuilder.Append($"JetMVAClassTrainingResult");
                var jobNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");
                if (string.IsNullOrWhiteSpace(jobNumber))
                {
                    jobNumber = "local";
                }
                jobNameBuilder.Append($"-{jobNumber}");

                var jobName = jobNameBuilder.ToString();
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
                var avoidPlaces = new[] { "Local", "UWTeV" };
                var trainingResultDir = outputHistograms.mkdir("Results");
                var tags = new string[] { "mc15c", "signal", "hss" }.Add(options.SmallTestingMenu ? "quick_compare" : "compare");
                var signalTestSources = SampleMetaData.AllSamplesWithTag(tags.ToArray())
                    .Select(info => (name: info.NickName, file: Files.GetSampleAsMetaData(info, avoidPlaces: avoidPlaces, weightByCrossSection: false)));
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

                // LLP Training
                Console.WriteLine("Fetching HSS Sample");
                var llp_training = SampleMetaData.AllSamplesWithTag("mc15c", "signal", "train", "hss")
                    .TakeEventsFromSamlesEvenly(options.EventsToUseForSignalTraining, Files.NFiles * 2,
                        mdQueriable => mdQueriable
                                        .AsGoodJetStream(options.pTCut, maxPtCut: TrainingUtils.MaxJetPtForTraining)
                                        .FilterSignal(options.LxyCut * 1000.0, options.LzCut * 1000.0),
                        weightByCrossSection: false, avoidPlaces: avoidPlaces);
                GenerateEfficiencyPlots(trainingResultDir.mkdir("training_hss"), llp_training.AsTrainingTree(),
                    cBDT, new string[] { "hss", "multijet", "bib" });

                // Multijet training
                Console.WriteLine("Fetching JZ Sample");
                var mj_training = BuildBackgroundTrainingTreeDataSource(options.EventsToUseForJzTraining,
                    options.pTCut, Files.NFiles, maxPtCut: TrainingUtils.MaxJetPtForTraining,
                    weightByCrossSection: true, avoidPlaces: avoidPlaces);
                GenerateEfficiencyPlots(trainingResultDir.mkdir("training_mj"), mj_training,
                    cBDT, new string[] { "hss", "multijet", "bib" });

                // Do do background and bib we need to force the data onto the non-local root stuff as the training happens with a more advanced
                // version of root than we have locally on windows.
                var bib15 = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB15, DataEpoc.data15, options.pTCut,
                    avoidPlaces: new[] { "Local", "UWTeV" },
                    useLessSamples: !options.UseFullDataset);
                var bib16 = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB16, DataEpoc.data16, options.pTCut,
                    avoidPlaces: new[] { "Local", "UWTeV" },
                    useLessSamples: !options.UseFullDataset);

                GenerateEfficiencyPlots(trainingResultDir.mkdir("data15"), bib15.AsTrainingTree(), cBDT, new string[] { "hss", "multijet", "bib" });
                GenerateEfficiencyPlots(trainingResultDir.mkdir("data16"), bib16.AsTrainingTree(), cBDT, new string[] { "hss", "multijet", "bib" });

                var multijet = BuildBackgroundTrainingTreeDataSource(options.EventsToUseForJzTraining, options.pTCut,
                    Files.NFiles, avoidPlaces: new[] { "Local", "UWTeV" });
                GenerateEfficiencyPlots(trainingResultDir.mkdir("jz"), multijet, cBDT, new string[] { "hss", "multijet", "bib" });

#if false
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

                var nnAvgSig = signalInCalOnly
                    .AsTrainingTree()
                    .FilterNonTrainingEvents()
                    .CalcTrainingError(m1, 0, 1.0);
                var nnAvgBack = backgroundTrainingTree
                    .FilterNonTrainingEvents()
                    .CalcTrainingError(m1, 1, 1.0);
                var nnAvgBiB = data15TrainingAndTesting == null
                    ? 0.5.AsFuture()
                    : data15TrainingAndTesting
                    .AsTrainingTree()
                    .FilterNonTrainingEvents()
                    .CalcTrainingError(m1, 2, 1.0);

                FutureWriteLine(() => $"The MVA cut signal efficiency of {options.PrecisionValue} is {nncutSig.Value}");
                FutureWriteLine(() => $"The MVA cut multijet efficiency of {options.PrecisionValue} is {nncutMultijet.Value}");
                FutureWriteLine(() => $"The MVA cut BIB efficiency of {options.PrecisionValue} is {nncutBiB.Value}");
                FutureWriteLine(() => $"The MVA average error for signal is {nnAvgSig.Value}");
                FutureWriteLine(() => $"The MVA average error for multijet is {nnAvgBack.Value}");
                FutureWriteLine(() => $"The MVA average error for BIB is {nnAvgBiB.Value}");
                var average = from nnSig in nncutSig
                              from nnMul in nncutMultijet
                              from nnBIB in nncutBiB
                              select (nnSig + nnMul + nnBIB) / 3.0;
                FutureWriteLine(() => $"The average MVA cut for {options.PrecisionValue} pass rate is {average.Value}");

                // Next, calc the 90% eff for each sample, and the average
                foreach (var s in signalTestSources)
                {
                    var interestingEvents = s.Item2
                        .AsGoodJetStream(options.pTCut)
                        .FilterSignal()
                        .AsTrainingTree()
                        .FilterNonTrainingEvents();
                    var nnCutTestSignal = interestingEvents
                        .FindNNCut(options.PrecisionValue, effhistDirectories, m1, 0, name: s.Item1);
                    var nnError = interestingEvents
                        .CalcTrainingError(m1, 0, 1.0);
                    FutureWriteLine(() => $"The MVA cut {s.Item1} efficiency of {options.PrecisionValue} is {nnCutTestSignal.Value}");
                    var a = from nnSig in nnCutTestSignal
                            from nnMul in nncutMultijet
                            from nnBIB in nncutBiB
                            select (nnSig + nnMul + nnBIB) / 3.0;
                    FutureWriteLine(() => $"  The average MVA cut for {s.Item1} for {options.PrecisionValue} pass rate is {a.Value}");
                    FutureWriteLine(() => $"  The MVA average error for {s.Item1} is {nnError.Value}");
                }
#endif
                // Done. Dump all output.
                Console.Out.DumpFutureLines();
            }
        }

        /// <summary>
        /// For easy wrigint out to a csv file
        /// </summary>
        public class WeightInfo
        {
            public int RunNumber { get; set; }
            public int EventNumber { get; set; }
            public double Weight { get; set; }
            public double WeightFlatten { get; set; }
            public double WeightMCEvent { get; set; }
            public double WeightXSection { get; set; }
            public float HSSWeight { get; set; }
            public float MultijetWeight { get; set; }
            public float BIBWeight { get; set; }
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
                .Select(j => Tuple.Create(cBDT.Invoke(j), j.Weight, j.RunNumber, j.EventNumber, j.WeightFlatten, j.WeightMCEvent, j.WeightXSection));

            // Generate plots
            foreach (var cinfo in trainingClassNames.Zip(Enumerable.Range(0, trainingClassNames.Length), (name, index) => Tuple.Create(name, index)))
            {
                s1
                    .Select(n => Tuple.Create((double) n.Item1[cinfo.Item2], n.Item2))
                    .FuturePlot(ClassifierEventWeight.NameFormat, ClassifierEventWeight.TitleFormat, ClassifierEventWeight, $"weight_{cinfo.Item1}")
                    .Save(outh);
            }

            // Generate a weight csv file
            var files = s1
                .Select(e => new WeightInfo()
                {
                    RunNumber = e.Item3,
                    EventNumber = e.Item4,
                    Weight = e.Item2,
                    WeightFlatten = e.Item5,
                    WeightMCEvent = e.Item6,
                    WeightXSection = e.Item7,
                    HSSWeight = e.Item1[0],
                    MultijetWeight = e.Item1[1],
                    BIBWeight = e.Item1[2]
                })
                .AsCSV(new FileInfo($"{outh.Directory.Name}.csv"));

            // Copy the CSV files into a single file.
            if (files.Length > 0)
            {
                var firstFile = files[0];
                var finalFile = new FileInfo($"all-{outh.Directory.Name}.csv");
                firstFile.CopyTo(finalFile.FullName, overwrite: true);

                if (files.Length > 1)
                {
                    foreach (var f in files.Skip(1))
                    {
                        File.AppendAllLines(finalFile.FullName, File.ReadLines(f.FullName).Skip(1));
                    }
                }
            }
        }
    }
}
