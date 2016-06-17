using DiVertAnalysis;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Linq;
using static libDataAccess.PlotSpecifications;
using static libDataAccess.Files;
using libDataAccess.Utils;
using System.Collections.Generic;

namespace JZPlotter
{
    /// <summary>
    /// This is a mostly experimental program to look at how to combine JZ samples into a single background.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Get command line arguments
            libDataAccess.Utils.CommandLineUtils.Parse(args);

            // Build our own set of background samples so we can experiment.
            var jets = new IQueryable<MetaData>[] {
                GetJZ(2),
                GetJZ(3),
                GetJZ(4),
            };

            // Count them individually
            var individualCounts = jets.Select(sample => sample.FutureCount()).ToArray();
            var firstsum = from c1 in individualCounts[0] from c2 in individualCounts[1] select c1 + c2;
            var sum = individualCounts.Skip(2).Aggregate(firstsum, (tot, val) => from t in tot from v in val select t + v);

            // Get the samples with an official weight attached to them.
            var allSamplesToTest = new List<Tuple<String, IQueryable<MetaData>>>()
            {
                Tuple.Create("AllJZ", GetAllJetSamples()),
                Tuple.Create("J2Z", GetJZ(2)),
                Tuple.Create("J3Z", GetJZ(3)),
                Tuple.Create("J4Z", GetJZ(4))
            };

            var totalCount = GetAllJetSamples().FutureCount();

            // Make a pT plot
            using (var outputHistograms = new FutureTFile("JZPlotter.root"))
            {
                foreach (var sampleInfo in allSamplesToTest)
                {
                    var events = sampleInfo.Item2;
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
