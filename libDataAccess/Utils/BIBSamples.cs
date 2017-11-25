using libDataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static libDataAccess.Utils.SampleUtils;

namespace libDataAccess.Utils
{
    /// <summary>
    /// Routines to help with getting and working with BIB samples
    /// </summary>
    public static class BIBSamples
    {
        /// <summary>
        /// Grab the BIB samles
        /// </summary>
        /// <param name="requestedNumberOfEvents">-1 for everything, or a number of requested</param>
        /// <param name="bib_tag">The tag name we should use to do the lookup</param>
        /// <returns></returns>
        public static IQueryable<JetStream> GetBIBSamples(int requestedNumberOfEvents, DataEpoc epoc, double pTCut, string[] avoidPlaces = null)
        {
            // If no events, then we need to just return everything
            if (requestedNumberOfEvents == 0)
            {
                return null;
            }

            // Parse the arguments into something more useful for pulling up the various datasets.
            var tag = epoc == DataEpoc.data15 ? "data15_p2950" : "data16_p2950";

            // If we are doing nFiles something other than zero, then we should
            // boost it. This is becaes a single file just isn't enough events that pass our
            // basic criteria in this dataset.
            var filesToAskFor = Files.NFiles == 0
                ? 0
                : Files.NFiles * 2;

            // If we have no restirction on number of events - then we can take everything.
            if (requestedNumberOfEvents < 0)
            {
                // Put the avoid places into a argument in the Uri
                var placesToAvoid = avoidPlaces?.Aggregate("", (acc, p) => acc + (acc.Length > 0 ? "," : "") + p);
                var placesToAvoidTag = placesToAvoid == null ? "" : $"&avoidPlaces={placesToAvoid}";

                // Since everything is evenly weighted, just grab everything.
                var tagUri = new Uri($"tagcollection://{tag}?nFilesPerSample={filesToAskFor}{placesToAvoidTag}");
                var queriable = DiVertAnalysis.QueryablerecoTree.CreateQueriable(new[] { tagUri });
                return Files.GenerateStream(queriable, 1.0)
                    .AsBeamHaloStream(epoc)
                    .AsGoodJetStream(pTCut);
            }

            // We have a limit on the number of events. Distribute our ask over the various samples so that we can have
            // events from early and late in the run where lumi profiles are different.
            var dataSamples = SampleMetaData.AllSamplesWithTag(tag);
            return dataSamples.TakeEventsFromSamlesEvenly(requestedNumberOfEvents, filesToAskFor,
                qm => qm.AsBeamHaloStream(epoc).AsGoodJetStream(pTCut));
        }

    }
}
