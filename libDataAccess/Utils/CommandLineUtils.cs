using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static libDataAccess.Files;

namespace libDataAccess.Utils
{
    /// <summary>
    /// Handle the basic (common) command line options.
    /// </summary>
    public class CommandLineUtils
    {
        public class Options
        {
            [Option("UseFullDataset", Default = false, HelpText = "Use full dataset rather than test dataset size.")]
            public bool UseFullDataset { get; set; }

            [Option("VerboseFileFetch", Default = false, HelpText = "Dump out details during file GRID access.")]
            public bool VerboseFileFetch { get; set; }

            [Option("UseBackgroundAll", Default = true, SetName = "backgroundsamples")]
            public bool BackgroundAll { get; set; }

            [Option("UseBackgroundJZ2", Default = false, SetName = "backgroundsamples")]
            public bool BackgroundJZ2 { get; set; }

            [Option("UseBackgroundJZ3", Default = false, SetName = "backgroundsamples")]
            public bool BackgroundJZ3 { get; set; }

            [Option("UseBackgroundJZ4", Default = false, SetName = "backgroundsamples")]
            public bool BackgroundJZ4 { get; set; }

            [Option("BDTMaxDepth", Default = 3)]
            public int BDTMaxDepth { get; set; }

            [Option("BDTLeafMinFraction", Default = 5)]
            public double BDTLeafMinFraction { get; set; }

            [Option("TrainingEvents", Default = 500000)]
            public int EventsToUseForTrainingAndTesting { get; set; }

            [Option("VariableTransform", Default = "")]
            public string VariableTransform { get; set; }

            [Option("UseCPPOptimizer", Default = 1)]
            public int UseCPPOptimizer { get; set; }

            [Option("TrainingVariableSet", Default = TrainingVariableSet.Default5pT)]
            public TrainingVariableSet TrainingVariableSet { get; set; }
        }

        enum BackgroundSampleEnum
        {
            All, JZ2, JZ3, JZ4
        }

        /// <summary>
        /// Possible variables for training dataset
        /// </summary>
        public enum TrainingVariableSet
        {
            Default5pT,
            Default5ET
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
        }

        /// <summary>
        /// What sample are we after?
        /// </summary>
        private static BackgroundSampleEnum RequstedBackgroundSample = BackgroundSampleEnum.All;

        /// <summary>
        /// Fetch the requested background depending on the signal.
        /// </summary>
        /// <returns></returns>
        public static IQueryable<MetaData> GetRequestedBackground()
        {
            switch (RequstedBackgroundSample)
            {
                case BackgroundSampleEnum.All:
                    return Files.GetAllJetSamples();

                case BackgroundSampleEnum.JZ2:
                    return GetJ2Z();

                case BackgroundSampleEnum.JZ3:
                    return GetJ3Z();

                case BackgroundSampleEnum.JZ4:
                    return GetJ4Z();

                default:
                    throw new InvalidOperationException("Unknown background samples");
            }
        }

        /// <summary>
        /// Get the list of background samples depending on the option that was given to us.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Tuple<string, IQueryable<MetaData>>> GetRequestedBackgroundSourceList()
        {
            switch (RequstedBackgroundSample)
            {
                case BackgroundSampleEnum.All:
                    return new Tuple<string, IQueryable<MetaData>>[] {
                        Tuple.Create("J2Z", GetJ2Z()),
                        Tuple.Create("J3Z", GetJ3Z()),
                        Tuple.Create("J4Z", GetJ4Z()),
                    };

                case BackgroundSampleEnum.JZ2:
                    return new Tuple<string, IQueryable<MetaData>>[] {
                        Tuple.Create("J2Z", GetJ2Z()),
                    };

                case BackgroundSampleEnum.JZ3:
                    return new Tuple<string, IQueryable<MetaData>>[] {
                        Tuple.Create("J3Z", GetJ3Z()),
                    };

                case BackgroundSampleEnum.JZ4:
                    return new Tuple<string, IQueryable<MetaData>>[] {
                        Tuple.Create("J4Z", GetJ4Z()),
                    };

                default:
                    throw new InvalidOperationException("Unknown background samples");
            }
        }

        /// <summary>
        /// The max depth for BDT training
        /// </summary>
        public static int MaxBDTDepth = 3;

        /// <summary>
        /// Min fraction of total evens that can be put into each leaf. It is a percentage.
        /// </summary>
        public static double BDTLeafMinFraction = 5;

        public static string TrainingVariableTransform = "";

        /// <summary>
        /// How many events to use for training/testing.
        /// </summary>
        public static int TrainingEvents = 500000;

        /// <summary>
        /// Which set list are we going to use?
        /// </summary>
        public static TrainingVariableSet TrainingVariableSetList { get; private set; }

        /// <summary>
        /// Parse the command line arguments, and deal with their execution.
        /// </summary>
        /// <param name="args"></param>
        public static void Parse(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);

            result.MapResult(
                options => {
                    Files.NFiles = options.UseFullDataset ? 0 : 1;
                    Files.VerboseFileFetch = options.VerboseFileFetch;
                    Files.UseCodeOptimizer = options.UseCPPOptimizer != 0;
                    if (options.BackgroundAll) RequstedBackgroundSample = BackgroundSampleEnum.All;
                    if (options.BackgroundJZ2) RequstedBackgroundSample = BackgroundSampleEnum.JZ2;
                    if (options.BackgroundJZ3) RequstedBackgroundSample = BackgroundSampleEnum.JZ3;
                    if (options.BackgroundJZ4) RequstedBackgroundSample = BackgroundSampleEnum.JZ4;
                    TrainingVariableSetList = options.TrainingVariableSet;
                    MaxBDTDepth = options.BDTMaxDepth;
                    BDTLeafMinFraction = options.BDTLeafMinFraction;
                    TrainingVariableTransform = options.VariableTransform;
                    TrainingEvents = options.EventsToUseForTrainingAndTesting;
                    return 0;
                },
                errors => {
                    foreach (var err in errors)
                    {
                        Console.WriteLine($"Error parsing command line: {err.ToString()}");
                    }
                    return 1;
                });
        }
    }
}
