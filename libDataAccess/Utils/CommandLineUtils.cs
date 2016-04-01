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
        }

        enum BackgroundSampleEnum
        {
            All, JZ2, JZ3, JZ4
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
                    return Files.GenerateStream(libDataAccess.Files.GetJ2Z(), 1.0);

                case BackgroundSampleEnum.JZ3:
                    return Files.GenerateStream(libDataAccess.Files.GetJ3Z(), 1.0);

                case BackgroundSampleEnum.JZ4:
                    return Files.GenerateStream(libDataAccess.Files.GetJ4Z(), 1.0);

                default:
                    throw new InvalidOperationException("Unknown background samples");
            }
        }

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
                    if (options.BackgroundAll) RequstedBackgroundSample = BackgroundSampleEnum.All;
                    if (options.BackgroundJZ2) RequstedBackgroundSample = BackgroundSampleEnum.JZ2;
                    if (options.BackgroundJZ3) RequstedBackgroundSample = BackgroundSampleEnum.JZ3;
                    if (options.BackgroundJZ4) RequstedBackgroundSample = BackgroundSampleEnum.JZ4;
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
