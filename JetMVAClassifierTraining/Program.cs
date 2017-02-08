using CommandLine;
using libDataAccess;
using libDataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CalRatioTMVAUtilities.PtReweightUtils;
using static CalRatioTMVAUtilities.TrainingVariableUtils;
using static libDataAccess.Utils.CommandLineUtils;
using static CalRatioTMVAUtilities.BackgroundSampleUtils;
using static libDataAccess.Utils.SampleUtils;
using LINQToTreeHelpers.FutureUtils;
using CalRatioTMVAUtilities;
using TMVAUtilities;

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
        }

        static void Main(string[] args)
        {
            // Parse command line arguments
            var options = CommandLineUtils.ParseOptions<Options>(args);

            // Class: LLP
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

            // Class: Multijet
            var backgroundTrainingTree = BuildBackgroundTrainingTreeDataSource(options.EventsToUseForTrainingAndTesting);

            // Class: BIB
            var data15 = SampleMetaData.AllSamplesWithTag("data15")
                .Take(options.UseFullDataset ? 10000 : 1)
                .SamplesAsSingleQueriable()
                .AsBeamHaloStream(DataEpoc.data15)
                .AsGoodJetStream();

            var data15TrainingAndTesting = data15;

            var data16 = SampleMetaData.AllSamplesWithTag("data16")
                .Take(options.UseFullDataset ? 10000 : 1)
                .SamplesAsSingleQueriable()
                .AsBeamHaloStream(DataEpoc.data16)
                .AsGoodJetStream();

            var data16TrainingAndTesting = data16;

            // The file we will use to dump everything about this training.
            using (var outputHistograms = new FutureTFile("JetMVAClassifierTraining.root"))
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
                    .AsClass("hss", isTrainingEvent: e => !(e.EventNumber % 3 == 1))
                    .EventClass(flatBackgroundTrainingData, "Multijet", isTrainingEvent: e => !(e.EventNumber % 3 == 1))
                    .EventClass(flatData16, "BIB", isTrainingEvent: e => !(e.EventNumber % 3 == 1))
                    .EventClass(flatData15, "BIB", isTrainingEvent: e => !(e.EventNumber % 3 == 1))
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

                // Done. Dump all output.
                Console.Out.DumpFutureLines();
            }
        }
    }
}
