using CommandLine;
using libDataAccess;
using libDataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CalRatioTMVAUtilities.BackgroundSampleUtils;
using static CalRatioTMVAUtilities.PtReweightUtils;
using static CalRatioTMVAUtilities.TrainingVariableUtils;
using static libDataAccess.PlotSpecifications;
using static libDataAccess.Utils.CommandLineUtils;
using static libDataAccess.Utils.SampleUtils;
using static libDataAccess.Utils.FutureConsole;
using LINQToTreeHelpers.FutureUtils;
using CalRatioTMVAUtilities;
using LINQToTTreeLib.Files;
using System.IO;

namespace MVADumpTrainingTuples
{
    class Program
    {
        /// <summary>
        /// Options for dumping out the training ntuples
        /// </summary>
        public class Options : CommonOptions
        {
            [Option("TrainingEvents", Default = 500000)]
            public int EventsToUseForTrainingAndTesting { get; set; }

            [Option("FlattenBy", Default = TrainingSpectraFlatteningPossibilities.JetPt)]
            public TrainingSpectraFlatteningPossibilities FlattenBy { get; set; }

            [Option("TrainEventsBIB16", HelpText = "How many events from data16 should be used in the training for bib16 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB16 { get; set; }

            [Option("TrainEventsBIB15", HelpText = "How many events from data15 should be used in the training for bib15 (-1 is all, 0 is none)?", Default = -1)]
            public int EventsToUseForTrainingAndTestingBIB15 { get; set; }

            [Option("PrecisionValue", HelpText = "The fraction of events in each sample to use when calculating the training precision", Default = 0.90)]
            public double PrecisionValue { get; set; }

            [Option("pTCut", HelpText = "The pT cut for jets in GeV. Defaults to 40.", Default = 40.0)]
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
            var data15TrainingAndTesting = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB15 < 0
                ? (options.UseFullDataset ? -1 : 25000)
                : options.EventsToUseForTrainingAndTestingBIB15
                , DataEpoc.data15, options.pTCut);
            var data16TrainingAndTesting = GetBIBSamples(options.EventsToUseForTrainingAndTestingBIB16 < 0
                ? (options.UseFullDataset ? -1 : 25000)
                : options.EventsToUseForTrainingAndTestingBIB16,
                DataEpoc.data16, options.pTCut);


            // The file we will use to dump everything about this training.
            using (var outputHistograms = new FutureTFile("MVADumpTrainingTuples.root"))
            {
                // Flatten everything as needed.
                var toMakeFlat = BuildFlatteningExpression(options.FlattenBy);

                var backgroundTrees = FlattenTrainingTree(backgroundTrainingTree, outputHistograms, toMakeFlat)
                    .AsTTree("DataTree", "Multijet Training Tree", new FileInfo("multijet.root"));
                CopyFilesOver(backgroundTrees, "multijet");

                var flatSignalTrainingData = FlattenTrainingTree(signalInCalOnly.AsTrainingTree(), outputHistograms, toMakeFlat)
                    .AsTTree("DataTree", "Signal Training Tree", new FileInfo("signal.root"));
                CopyFilesOver(flatSignalTrainingData, "signal");


                if (options.EventsToUseForTrainingAndTestingBIB15 != 0)
                {
                    var flatData15 = FlattenTrainingTree(data15TrainingAndTesting.AsTrainingTree(), outputHistograms, toMakeFlat)
                        .AsTTree("DataTree", "BIB15 Training Tree", new FileInfo("bib15.root"));
                    CopyFilesOver(flatData15, "bib15");
                }

                if (options.EventsToUseForTrainingAndTestingBIB16 != 0)
                {
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
            var dataSamples = SampleMetaData.AllSamplesWithTag(epoc == DataEpoc.data15 ? "data15_p2950" : "data16_p2950");

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

            // Check that we did ok. This will prevent errors down the line that are rather confusing.
            if (countOfEvents < requestedNumberOfEvents)
            {
                Console.WriteLine($"Warning - unable to get all the events requested for {epoc.ToString()}. {countOfEvents} were found, and {requestedNumberOfEvents} events were requested.");
            }
            if (countOfEvents == 0 && requestedNumberOfEvents > 0)
            {
                throw new InvalidOperationException($"Unable to get any events for {epoc.ToString()}!");
            }

            return data;
        }
    }
}