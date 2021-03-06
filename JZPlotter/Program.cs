﻿using DiVertAnalysis;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Linq;
using static libDataAccess.PlotSpecifications;
using static libDataAccess.Files;
using libDataAccess.Utils;
using System.Collections.Generic;
using libDataAccess;
using static libDataAccess.Utils.CommandLineUtils;
using System.Threading.Tasks;

namespace JZPlotter
{
    /// <summary>
    /// This is a mostly experimental program to look at how to combine JZ samples into a single background.
    /// </summary>
    class Program
    {
        /// <summary>
        /// For options unique to this program
        /// </summary>
        class Options : CommonOptions
        {

        }

        static async Task Main(string[] args)
        {
            // Get command line arguments
            var opt = ParseOptions<Options>(args);

            // Build our own set of background samples so we can experiment.
            var jets = await Task.WhenAll(SampleMetaData.AllSamplesWithTag("background")
                .Select(info => Files.GetSampleAsMetaData(info)));

            // Count them individually
            var individualCounts = jets.Select(sample => sample.FutureCount()).ToArray();
            var firstsum = from c1 in individualCounts[0] from c2 in individualCounts[1] select c1 + c2;
            var sum = individualCounts.Skip(2).Aggregate(firstsum, (tot, val) => from t in tot from v in val select t + v);

            // Get the samples with an official weight attached to them.
            var allSamplesToTest =
                new[] { Tuple.Create("JZAll", GetAllJetSamples()) }
                .Concat(SampleMetaData.AllSamplesWithTag("background").Select(info => Tuple.Create(info.NickName, Files.GetSampleAsMetaData(info))))
                .ToArray();

            var totalCount = (await GetAllJetSamples()).FutureCount();

            // Make a pT plot
            using (var outputHistograms = new FutureTFile("JZPlotter.root"))
            {
                foreach (var sampleInfo in allSamplesToTest)
                {
                    var events = await sampleInfo.Item2;
                    var hdir = outputHistograms.mkdir(sampleInfo.Item1);

                    events
                        .Select(e => e.Data.eventWeight)
                        .FuturePlot("event_weights", "Sample EventWeights", 100, 0.0, 1000.0)
                        .Save(hdir);

                    events
                        .AsGoodJetStream()
                        .FuturePlot(JetPtPlotJetStream.ResetWeight(), "pt_unweighted")
                        .Save(hdir);
                    events
                        .AsGoodFirstJetStream()
                        .FuturePlot(JetPtPlotJetStream.ResetWeight(), "first_pt_unweighted")
                        .Save(hdir);

                    events
                        .AsGoodJetStream()
                        .FuturePlot(JetPtPlotJetStream, "pt_weighted")
                        .Save(hdir);
                    events
                        .AsGoodFirstJetStream()
                        .FuturePlot(JetPtPlotJetStream, "first_jet_pt_weighted")
                        .Save(hdir);
                }
            }

            // Print out summary of numbers
            Console.WriteLine($"Sum by adding individually: {sum.Value}.");
            Console.WriteLine($"Sum when done all as one: {totalCount.Value}");
            foreach (var s in individualCounts)
            {
                Console.WriteLine($"  Sample Events: {s.Value}.");
            }
        }

        public static IQueryable<MetaData> GenerateStream (IQueryable<recoTree> source, double xSecWeight)
        {
            return source.Select(e => new MetaData() { Data = e, xSectionWeight = xSecWeight });
        }
    }
}
