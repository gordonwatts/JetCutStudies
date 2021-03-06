﻿using CommandLine;
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
        public class CommonOptions
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

            [Option("UseCPPOptimizer", Default = 1)]
            public int UseCPPOptimizer { get; set; }

            [Option("IgnoreQueryCache", Default = 0)]
            public int IgnoreQueryCache { get; set; }
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
        public static Task<IQueryable<MetaData>> GetRequestedBackground()
        {
            switch (RequstedBackgroundSample)
            {
                case BackgroundSampleEnum.All:
                    return Files.GetAllJetSamples();

                case BackgroundSampleEnum.JZ2:
                    return GetJZ(2);

                case BackgroundSampleEnum.JZ3:
                    return GetJZ(3);

                case BackgroundSampleEnum.JZ4:
                    return GetJZ(4);

                default:
                    throw new InvalidOperationException("Unknown background samples");
            }
        }

        /// <summary>
        /// Get the list of background samples depending on the option that was given to us.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<SampleMetaData> GetRequestedBackgroundSourceList()
        {
            switch (RequstedBackgroundSample)
            {
                case BackgroundSampleEnum.All:
                    return SampleMetaData.AllSamplesWithTag("jz", "background_p2952", "emma2");

                default:
                    throw new InvalidOperationException("Unknown background samples");
            }
        }

        /// <summary>
        /// From a list of samples, return them by name.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        private static Task<Tuple<string, IQueryable<MetaData>>[]> SamplesAsNamedSequence(IEnumerable<SampleMetaData> samples, string[] avoidPlaces = null)
        {
            return Task.WhenAll(samples
                            .Select(async s => Tuple.Create(s.Name, await Files.GetSampleAsMetaData(s, avoidPlaces: avoidPlaces)))
                   );
        }

        /// <summary>
        /// Parase and return a set of options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ParseOptions<T>(string[] args)
            where T : CommonOptions
        {
            var result = Parser.Default.ParseArguments<T>(args.Select(a => a.Trim()));

            T optVar = null;
            result.MapResult(
                options => {
                    Files.NFiles = options.UseFullDataset ? 0 : 1;
                    Files.VerboseFileFetch = options.VerboseFileFetch;
                    Files.UseCodeOptimizer = options.UseCPPOptimizer != 0;
                    Files.IgnoreQueires = options.IgnoreQueryCache != 0;
                    if (options.BackgroundAll) RequstedBackgroundSample = BackgroundSampleEnum.All;
                    if (options.BackgroundJZ2) RequstedBackgroundSample = BackgroundSampleEnum.JZ2;
                    if (options.BackgroundJZ3) RequstedBackgroundSample = BackgroundSampleEnum.JZ3;
                    if (options.BackgroundJZ4) RequstedBackgroundSample = BackgroundSampleEnum.JZ4;
                    optVar = options;
                    return 0;
                },
                errors => {
                    foreach (var err in errors)
                    {
                        Console.WriteLine($"Error parsing command line: {err.ToString()}");
                    }
                    throw new InvalidOperationException("Command line couldn't be parsed");
                });

            return optVar;
        }
    }
}
