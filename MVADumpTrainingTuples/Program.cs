using CalRatioTMVAUtilities;
using CommandLine;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib.Files;
using LINQToTTreeLib;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CalRatioTMVAUtilities.BackgroundSampleUtils;
using static CalRatioTMVAUtilities.PtReweightUtils;
using static libDataAccess.Utils.BIBSamples;
using static libDataAccess.Utils.CommandLineUtils;
using static libDataAccess.Utils.FutureConsole;
using static libDataAccess.Utils.SampleUtils;
using System.Linq.Expressions;

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

            [Option("TrainingEventsBIB16", HelpText = "How many events from data16 should be used in the training for bib16 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB16 { get; set; }

            [Option("TrainingEventsBIB15", HelpText = "How many events from data15 should be used in the training for bib15 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB15 { get; set; }

            [Option("PrecisionValue", HelpText = "The fraction of events in each sample to use when calculating the training precision", Default = 0.90)]
            public double PrecisionValue { get; set; }

            [Option("pTCut", HelpText = "The pT cut for jets in GeV.", Default = 40.0)]
            public double pTCut { get; set; }

            [Option("LxyCut", HelpText = "Restrict barrel signal to have a Lxy of at least this value (meters).", Default = 0.0)]
            public double LxyCut { get; set; }

            [Option("LzCut", HelpText = "Restrict endcap signal to have a Lxy of at least this value (meters)", Default = 0.0)]
            public double LzCut { get; set; }

            [Option("InputFile", HelpText = "Instead of default data sets, run on this particular file as a sample.")]
            public string InputFile { get; set; }
        }

        static async Task Main(string[] args)
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

            // Where we'd like to run
            var whereToRun = new[] { "UWTeV-linux", "CERNLLP-linux" };

            // The file we will use to dump everything about this training.
            using (var outputHistograms = new FutureTFile("MVADumpTrainingTuples.root"))
            {
                // Flatten everything as needed.
                var toMakeFlat = BuildFlatteningExpression(options.FlattenBy);

                if (options.InputFile == null)
                {
                    var allOut = new Task[] {WriteOutSignal(options.EventsToUseForSignalTraining, options.pTCut, options.LxyCut, options.LzCut, whereToRun, outputHistograms, toMakeFlat),
                    WriteOutMJ(options.EventsToUseForJzTraining, options.pTCut, whereToRun, outputHistograms, toMakeFlat),
                    WriteOutBIB(DataEpoc.data15, options.EventsToUseForTrainingAndTestingBIB15, options.pTCut, options.UseFullDataset, whereToRun, toMakeFlat, outputHistograms),
                    WriteOutBIB(DataEpoc.data16, options.EventsToUseForTrainingAndTestingBIB16, options.pTCut, options.UseFullDataset, whereToRun, toMakeFlat, outputHistograms) };
                    
                    await Task.WhenAll(allOut);
                } else
                {
                    // just do one file.
                    await WriteOutForFile(options.InputFile, options.pTCut, toMakeFlat, outputHistograms);
                }
            }

            // Done. Dump all output.
            Console.Out.DumpFutureLines();
            Console.WriteLine("Done!");
        }

        /// <summary>
        /// Write out results for a single file
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="pTCut"></param>
        /// <param name="toMakeFlat"></param>
        /// <param name="outputHistograms"></param>
        /// <returns></returns>
        private static async Task WriteOutForFile(string inputFile, double pTCut, Expression<Func<TrainingTree, double>> toMakeFlat, FutureTDirectory outputHistograms)
        {
            var sampleEvents = Files.FileAsStream(new FileInfo(inputFile))
                .AsGoodJetStream(pTCut, maxPtCut: TrainingUtils.MaxJetPtForTraining);

            var r = await (FlattenTrainingTree(sampleEvents.AsTrainingTree(), outputHistograms, toMakeFlat)
                .FutureAsCSV(new FileInfo("fileout.csv")));
            CopyFilesOver(r, "data");
        }

        /// <summary>
        /// Write out the signal to a CSV file.
        /// </summary>
        /// <returns></returns>
        private static async Task WriteOutSignal(int eventsToUseForSignalTraining,
            double pTCut, double LxyCut, double LzCut,
            string[] whereToRun, FutureTDirectory histOutput,
            Expression<Func<TrainingTree,double>> toMakeFlat)
        {
            if (eventsToUseForSignalTraining > 0)
            {
                // Class: LLP
                Console.WriteLine("Fetching HSS Sample");
                var signalInCalOnly = SampleMetaData.AllSamplesWithTag("signal_p2952", "emma", "train", "hss")
                    .TakeEventsFromSamlesEvenly(eventsToUseForSignalTraining, Files.NFiles * 2,
                        mdQueriable => mdQueriable.AsGoodJetStream(pTCut, maxPtCut: TrainingUtils.MaxJetPtForTraining).FilterSignal(LxyCut * 1000.0, LzCut * 1000.0),
                        weightByCrossSection: false, preferPlaces: whereToRun);

                Console.WriteLine("Writing out csv files for signal.");
                var flatSignalTrainingData = FlattenTrainingTree((await signalInCalOnly).AsTrainingTree(), histOutput, toMakeFlat)
                    .FutureAsCSV(new FileInfo("signal.csv"));
                CopyFilesOver(await flatSignalTrainingData, "signal");
            }
        }

        private static async Task WriteOutMJ(int eventsToUseForJzTraining, double pTCut,
            string[] whereToRun, FutureTDirectory outputHistograms,
            Expression<Func<TrainingTree, double>> toMakeFlat)
        {
            if (eventsToUseForJzTraining > 0)
            {
                Console.WriteLine("Fetching JZ Sample");
                var backgroundTrainingTree = await BuildBackgroundTrainingTreeDataSource(eventsToUseForJzTraining,
                pTCut, Files.NFiles, maxPtCut: TrainingUtils.MaxJetPtForTraining,
                preferPlaces: whereToRun);

                if (backgroundTrainingTree == null)
                {
                    throw new InvalidOperationException("Despite requeted MJ events, we found none! This is pretty bad!");
                }

                Console.WriteLine("Writing out csv files for multijet.");
                var backgroundTrees = FlattenTrainingTree(backgroundTrainingTree, outputHistograms, toMakeFlat)
                    .FutureAsCSV(new FileInfo("multijet.csv"));
                CopyFilesOver(await backgroundTrees, "multijet");

            }
        }

        public static async Task WriteOutBIB(DataEpoc epoc, int eventsToUse, double pTCut, bool useFullDataset,
            string[] whereToRun, Expression<Func<TrainingTree,double>> toMakeFlat,
            FutureTDirectory outputHistograms)
        {
            Console.WriteLine("Fetching BIB15 Sample");
            var data15TrainingAndTesting = await GetBIBSamples(eventsToUse, epoc, pTCut,
                maxPtCut: TrainingUtils.MaxJetPtForTraining, 
                useLessSamples: !useFullDataset, preferPlaces: whereToRun);

            if (data15TrainingAndTesting != null)
            {
                var stub = epoc == DataEpoc.data15
                    ? "bib15"
                    : "bib16";

                Console.WriteLine("Writing out csv files for BIB15.");
                var flatData15 = FlattenTrainingTree(data15TrainingAndTesting.AsTrainingTree(), outputHistograms, toMakeFlat)
                    .FutureAsCSV(new FileInfo($"{stub}.csv"));
                CopyFilesOver(await flatData15, stub);
            }

        }

        /// <summary>
        /// Copy the files over to somethign reasonable (for pick up by Jenkins and other things).
        /// </summary>
        /// <param name="treeFiles">Files that contains the trees</param>
        /// <param name="finalNameRoot">Base filename to copy over to</param>
        private static void CopyFilesOver(FileInfo[] treeFiles, string finalNameRoot)
        {
            var items = treeFiles
                .Zip(Enumerable.Range(0, 10000), (f, r) => (file: f, index: r))
                .Select(f => (original: f.file, newfile: new FileInfo($"{f.file.Directory.FullName}\\{finalNameRoot}-{f.index:D4}{f.file.Extension}")));
            foreach (var f in items)
            {
                f.original.CopyTo(f.newfile.FullName, overwrite: true);
            }
        }
    }
}