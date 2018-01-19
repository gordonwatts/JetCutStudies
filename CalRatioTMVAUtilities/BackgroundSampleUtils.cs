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
        public static async Task<IQueryable<TrainingTree>> BuildBackgroundTrainingTreeDataSource(int eventsToUseForTrainingAndTesting, double pTCut = 40.0,
            int numberOfFiles = 0, bool useOnlyOneSample = false,
            string[] avoidPlaces = null, string[] preferPlaces = null,
            bool weightByCrossSection = true, double? maxPtCut = null)
        {
            // Get the number of events in each source.
            return await CommandLineUtils.GetRequestedBackgroundSourceList(avoidPlaces)
                .TakeEventsFromSamlesEvenly(eventsToUseForTrainingAndTesting, numberOfFiles,
                    qm => qm.AsGoodJetStream(pTCut, maxPtCut).AsTrainingTree(), weightByCrossSection: weightByCrossSection,
                    avoidPlaces: avoidPlaces);
        }
    }
}
