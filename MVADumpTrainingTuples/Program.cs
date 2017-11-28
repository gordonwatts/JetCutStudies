using CalRatioTMVAUtilities;
using CommandLine;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib.Files;
using System;
using System.IO;
using System.Linq;
using static CalRatioTMVAUtilities.BackgroundSampleUtils;
using static CalRatioTMVAUtilities.PtReweightUtils;
using static libDataAccess.Utils.BIBSamples;
using static libDataAccess.Utils.CommandLineUtils;
using static libDataAccess.Utils.FutureConsole;
using static libDataAccess.Utils.SampleUtils;

namespace MVADumpTrainingTuples
{
    class Program
    {
        /// <summary>
        /// Options for dumping out the training ntuples
        /// </summary>
        public class Options : CommonOptions
        {
            [Option("FlattenBy", Default = TrainingSpectraFlatteningPossibilities.JetPt)]
            public TrainingSpectraFlatteningPossibilities FlattenBy { get; set; }

            [Option("TrainingEventsJz", Default = -1, HelpText = "Number of events to use in training for JZ sample. -1 means everything. Defaults to 20,000 if UseFullDataset not presen.")]
            public int EventsToUseForJzTraining { get; set; }

            [Option("TrainingEventsSignal", Default = -1, HelpText = "Number of events to use in training for singal sample. -1 means everything. Defaults to 20,000 if UseFullDataset not presen.")]
            public int EventsToUseForSignalTraining { get; set; }

            [Option("TrainEventsBIB16", HelpText = "How many events from data16 should be used in the training for bib16 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB16 { get; set; }

            [Option("TrainEventsBIB15", HelpText = "How many events from data15 should be used in the training for bib15 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB15 { get; set; }

            [Option("PrecisionValue", HelpText = "The fraction of events in each sample to use when calculating the training precision", Default = 0.90)]
            public double PrecisionValue { get; set; }

            [Option("pTCut", HelpText = "The pT cut for jets in GeV.", Default = 40.0)]
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
                .TakeEventsFromSamlesEvenly(options.EventsToUseForSignalTraining, Files.NFiles * 2,
                    mdQueriable => mdQueriable.AsGoodJetStream(options.pTCut, maxPtCut: TrainingUtils.MaxJetPtForTraining).FilterSignal(options.LxyCut * 1000.0, options.LzCut * 1000.0), weightByCrossSection: false);

            // Class: Multijet
            Console.WriteLine("Fetching JZ Sample");
            var backgroundTrainingTree = BuildBackgroundTrainingTreeDataSource(options.EventsToUseForJzTraining,
                options.pTCut, Files.NFiles, maxPtCut: TrainingUtils.MaxJetPtForTraining);

            // Class: BIB
            Console.WriteLine("Fetching BIB15 Sample");
            var data15TrainingAndTesting = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB15, DataEpoc.data15, options.pTCut, maxPtCut: TrainingUtils.MaxJetPtForTraining, useLessSamples: !options.UseFullDataset);
            Console.WriteLine("Fetching BIB16 Sample");
            var data16TrainingAndTesting = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB16, DataEpoc.data16, options.pTCut, maxPtCut: TrainingUtils.MaxJetPtForTraining, useLessSamples: !options.UseFullDataset);

            // The file we will use to dump everything about this training.
            using (var outputHistograms = new FutureTFile("MVADumpTrainingTuples.root"))
            {
                // Flatten everything as needed.
                var toMakeFlat = BuildFlatteningExpression(options.FlattenBy);

                Console.WriteLine("Writing out csv files for multijet.");
                var backgroundTrees = FlattenTrainingTree(backgroundTrainingTree, outputHistograms, toMakeFlat)
                    .AsTTree("DataTree", "Multijet Training Tree", new FileInfo("multijet.root"));
                CopyFilesOver(backgroundTrees, "multijet");

                Console.WriteLine("Writing out csv files for signal.");
                var flatSignalTrainingData = FlattenTrainingTree(signalInCalOnly.AsTrainingTree(), outputHistograms, toMakeFlat)
                    .AsTTree("DataTree", "Signal Training Tree", new FileInfo("signal.root"));
                CopyFilesOver(flatSignalTrainingData, "signal");


                if (data15TrainingAndTesting != null)
                {
                    Console.WriteLine("Writing out csv files for BIB15.");
                    var flatData15 = FlattenTrainingTree(data15TrainingAndTesting.AsTrainingTree(), outputHistograms, toMakeFlat)
                        .AsTTree("DataTree", "BIB15 Training Tree", new FileInfo("bib15.root"));
                    CopyFilesOver(flatData15, "bib15");
                }

                if (data16TrainingAndTesting != null)
                {
                    Console.WriteLine("Writing out csv files for BIB16.");
                    var flatData16 = FlattenTrainingTree(data16TrainingAndTesting.AsTrainingTree(), outputHistograms, toMakeFlat)
                        .AsTTree("DataTree", "BIB16 Training Tree", new FileInfo("bib16.root"));
                    CopyFilesOver(flatData16, "bib16");
                }
            }

            // Done. Dump all output.
            Console.Out.DumpFutureLines();
        }

        /// <summary>
        /// Copy the files over to somethign reasonable (for pick up by Jenkins and other things).
        /// </summary>
        /// <param name="treeFiles">Files that contains the trees</param>
        /// <param name="finalNameRoot">Base filename to copy over to</param>
        private static void CopyFilesOver(FileInfo[] treeFiles, string finalNameRoot)
        {
            var items = treeFiles
                .Zip(Enumerable.Range(0, 100), (f, r) => (file: f, index: r))
                .Select(f => (original: f.file, newfile: new FileInfo($"{f.file.Directory.FullName}\\{finalNameRoot}-{f.index:D2}.root")));
            foreach (var f in items)
            {
                f.original.CopyTo(f.newfile.FullName, overwrite: true);
            }
        }
    }
}