using libDataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalRatioTMVAUtilities
{
    public static class BackgroundSampleUtils
    {
        /// <summary>
        /// Generate the training data source.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Take the same fraction of events from each source.
        /// </remarks>
        public static IQueryable<TrainingTree> BuildBackgroundTrainingTreeDataSource(double eventsToUseForTrainingAndTesting, bool useOnlyOneSample = false)
        {
            // Get the number of events in each source.
            var backgroundSources = CommandLineUtils.GetRequestedBackgroundSourceList()
                .Take(useOnlyOneSample ? 1 : 1000);
            var backgroundEventsWithCounts = backgroundSources
                .Select(b => b.Item2.AsGoodJetStream().AsTrainingTree())
                .Select(b => Tuple.Create(b.Count(), b))
                .ToArray();

            // The fraction of weight we want from each source we will take.
            var sourceFraction = eventsToUseForTrainingAndTesting / backgroundEventsWithCounts.Select(e => e.Item1).Sum();
            sourceFraction = sourceFraction > 1.0 ? 1.0 : sourceFraction;

            // Build a stream of all the backgrounds, stitched together.
            return backgroundEventsWithCounts
                .Select(e => e.Item2.Take((int)(e.Item1 * sourceFraction)))
                .Aggregate((IQueryable<TrainingTree>)null, (s, add) => s == null ? add : s.Concat(add));
        }

    }
}
