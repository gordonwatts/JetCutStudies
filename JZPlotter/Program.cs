using DiVertAnalysis;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Linq;
using static libDataAccess.PlotSpecifications;
using static libDataAccess.Files;
using libDataAccess.Utils;

namespace JZPlotter
{
    public class MetaData
    {
        public recoTree Data;
        public double xSectionWeight;
    }

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
                GenerateStream(libDataAccess.Files.GetJ2Z(), 1.0),
                GenerateStream(libDataAccess.Files.GetJ3Z(), 1.0),
                GenerateStream(libDataAccess.Files.GetJ4Z(), 1.0),
            };

            // Count them individually
            var individualCounts = jets.Select(sample => sample.FutureCount()).ToArray();
            var firstsum = from c1 in individualCounts[0] from c2 in individualCounts[1] select c1 + c2;
            var sum = individualCounts.Skip(2).Aggregate(firstsum, (tot, val) => from t in tot from v in val select t + v);

            // Get the samples with an official weight attached to them.
            var allFilesNoWeight = GetAllJetSamples();

            var totalCount = allFilesNoWeight.FutureCount();

            // Make a pT plot
            using (var outputHistograms = new FutureTFile("JZPlotter.root"))
            {
                allFilesNoWeight
                    .AsGoodJetStream()
                    .FuturePlot(JetPtPlotJetStream, "pt_unweighted")
                    .Save(outputHistograms);
                allFilesNoWeight
                    .AsGoodFirstJetStream()
                    .FuturePlot(JetPtPlotJetStream, "first_pt_unweighted")
                    .Save(outputHistograms);

                allFilesNoWeight
                    .AsGoodJetStream()
                    .FuturePlot(JetPtPlotJetStream, "pt_weighted")
                    .Save(outputHistograms);
                allFilesNoWeight
                    .AsGoodFirstJetStream()
                    .FuturePlot(JetPtPlotJetStream, "first_jet_pt_weighted")
                    .Save(outputHistograms);
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
