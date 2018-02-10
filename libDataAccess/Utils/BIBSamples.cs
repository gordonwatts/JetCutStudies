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
        /// <remarks>
        /// If the flag for "useLessSamples" is set, then we will try to use only the first 10 samples.
        /// </remarks>
        public static async Task<IQueryable<JetStream>> GetBIBSamples(int requestedNumberOfEvents, DataEpoc epoc, double pTCut, string[] avoidPlaces = null,
            bool useLessSamples = false, double? maxPtCut = null, string[] preferPlaces = null)
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
            var filesToAskFor = Files.NFiles*14;
            //var filesToAskFor = Files.NFiles == 0
            //    ? 0
            //    : Files.NFiles * 2;

            // If we have no restirction on number of events - then we can take everything.
            if (requestedNumberOfEvents < 0)
            {
                throw new NotImplementedException("Don't know how to deal with second tag here yet");
                // Put the avoid places into a argument in the Uri
                var placesToAvoid = avoidPlaces?.Aggregate("", (acc, p) => acc + (acc.Length > 0 ? "," : "") + p);
                var placesToAvoidTag = placesToAvoid == null ? "" : $"&avoidPlaces={placesToAvoid}";

                var placesToGo = preferPlaces?.Aggregate("", (acc, p) => acc + (acc.Length > 0 ? "," : "") + p);
                var placesToGoTag = placesToGo == null ? "" : $"&preferPlaces={placesToGo}";

                // Since everything is evenly weighted, just grab everything.
                var tagUri = new Uri($"tagcollection://{tag}?nFilesPerSample={filesToAskFor}{placesToAvoidTag}{placesToGo}&jobName={Files.JobName}&jobVersion={Files.JobVersionNumber.ToString()}");
                var queriable = DiVertAnalysis.QueryablerecoTree.CreateQueriable(new[] { tagUri });
                return Files.GenerateStream(queriable, 1.0)
                    .AsBeamHaloStream(epoc)
                    .AsGoodJetStream(pTCut, maxPtCut);
            }

            // We have a limit on the number of events. Distribute our ask over the various samples so that we can have
            // events from early and late in the run where lumi profiles are different.
            var dataSamples = SampleMetaData.AllSamplesWithTag(tag, "emma");
            if (useLessSamples)
            {
                dataSamples = dataSamples.Take(20);
            }
            return await dataSamples.TakeEventsFromSamlesEvenly(requestedNumberOfEvents, filesToAskFor,
                qm => qm.AsBeamHaloStream(epoc).AsGoodJetStream(pTCut, maxPtCut), avoidPlaces: avoidPlaces, preferPlaces: preferPlaces, weightByCrossSection: false);
        }

    }
}
