using libDataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libDataAccess;

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
        public static IQueryable<TrainingTree> BuildBackgroundTrainingTreeDataSource(int eventsToUseForTrainingAndTesting, double pTCut = 40.0, int numberOfFiles = 0, bool useOnlyOneSample = false, string[] avoidPlaces = null, bool weightByCrossSection = true)
        {
            // Get the number of events in each source.
            return CommandLineUtils.GetRequestedBackgroundSourceList(avoidPlaces)
                .TakeEventsFromSamlesEvenly(eventsToUseForTrainingAndTesting, numberOfFiles,
                    qm => qm.AsGoodJetStream(pTCut).AsTrainingTree(), weightByCrossSection: weightByCrossSection);
        }
    }
}
