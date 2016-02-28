using DiVertAnalysis;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Linq;
using static libDataAccess.PlotSpecifications;

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
            var jets = new IQueryable<MetaData>[] {
                GenerateStream(libDataAccess.Files.GetJ2Z(), 1.0),
                GenerateStream(libDataAccess.Files.GetJ3Z(), 0.5),
                GenerateStream(libDataAccess.Files.GetJ4Z(), 0.25),
            };

            // Count them individually
            var individualCounts = jets.Select(sample => sample.FutureCount()).ToArray();
            var firstsum = from c1 in individualCounts[0] from c2 in individualCounts[1] select c1 + c2;
            var sum = individualCounts.Skip(2).Aggregate(firstsum, (tot, val) => from t in tot from v in val select t + v);

            // Count them by concatting them.
            var allFilesNoWeight = jets
                .Skip(1)
                .Aggregate(jets[0], (seq, newj) => seq.Concat(newj));

            var totalCount = allFilesNoWeight.FutureCount();

            // Make a pT plot
            using (var outputHistograms = new FutureTFile("JZPlotter.root"))
            {
                allFilesNoWeight
                    .SelectMany(e => e.Data.Jets)
                    .FuturePlot(JetPtPlot, "pt_unweighted")
                    .Save(outputHistograms);
                allFilesNoWeight
                    .Select(e => e.Data.Jets.OrderByDescending(j => j.pT).First())
                    .FuturePlot(JetPtPlot, "first_pt_unweighted")
                    .Save(outputHistograms);

                allFilesNoWeight
                    .SelectMany(e => e.Data.Jets.Select(j => new { Jet = j, Weight = e.xSectionWeight}))
                    .FuturePlot("pt_weighted", "Jet pT Weighted", 100, 0.0, 100.0, j => j.Jet.pT, j => j.Weight)
                    .Save(outputHistograms);
                allFilesNoWeight
                    .Select(e => e.Data.Jets.OrderByDescending(j => j.pT).Select(j => new { Jet = j, Weight = e.xSectionWeight }).First())
                    .FuturePlot("first_jet_pt_weighted", "Jet pT Weighted", 100, 0.0, 100.0, j => j.Jet.pT, j => j.Weight)
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
