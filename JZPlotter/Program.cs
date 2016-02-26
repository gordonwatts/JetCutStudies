using DiVertAnalysis;
using LINQToTTreeLib;
using LINQToTreeHelpers.FutureUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JZPlotter
{
    /// <summary>
    /// This is a mostly experimental program to look at how to combine JZ samples into a single background.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var jets = new IQueryable<recoTree>[] {
                libDataAccess.Files.GetJ2Z(),
                libDataAccess.Files.GetJ3Z(),
                libDataAccess.Files.GetJ4Z(),
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

            // Print out summary of numbers
            Console.WriteLine($"Sum by adding individually: {sum.Value}.");
            Console.WriteLine($"Sum when done all as one: {totalCount.Value}");
            foreach (var s in individualCounts)
            {
                Console.WriteLine($"  Sample Events: {s.Value}.");
            }
        }
    }
}
