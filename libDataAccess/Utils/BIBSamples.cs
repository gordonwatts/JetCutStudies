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
            if (avoidPlaces != null)
                throw new NotImplementedException();

            var tag = epoc == DataEpoc.data15 ? "data15_p2950" : "data16_p2950";
            var tagUri = new Uri($"tagcollection://{tag}");

            var queriable = DiVertAnalysis.QueryablerecoTree.CreateQueriable(new[] { tagUri });
            return Files.GenerateStream(queriable, 1.0)
                .AsBeamHaloStream(epoc)
                .AsGoodJetStream(pTCut);
#if false
            // If no events, then we need to just return everything
            if (requestedNumberOfEvents == 0)
            {
                return null;
            }

            // Fetch all the data samples
            var dataSamples = SampleMetaData.AllSamplesWithTag(epoc == DataEpoc.data15 ? "data15_p2950" : "data16_p2950");

            // BiB files are funny - the data is far and few between. So we need to boost the number of files
            // we look at.
            var oldNFiles = Files.NFiles;
            if (oldNFiles <= 1)
            {
                Files.NFiles = 2;
            }
            try
            {

                // If we have a limitation on the number of events, then we need to measure our the # of events.
                int countOfEvents = 0;
                int countOfEventsOneBack = 0;
                dataSamples = dataSamples
                    .TakeWhile(s =>
                    {
                        if (requestedNumberOfEvents < 0)
                        {
                            return true;
                        }
                        var q = Files.GetSampleAsMetaData(s, avoidPlaces: avoidPlaces);
                        countOfEventsOneBack = countOfEvents;
                        countOfEvents += q.AsBeamHaloStream(epoc)
                                            .AsGoodJetStream(pTCut)
                                            .Count();
                        return countOfEvents < requestedNumberOfEvents;
                    })
                    .ToArray();

                // The following is the tricky part. Now that we have a list of events, it is not likely that we have found a file boundary
                // that matches the number of events. So we will have to do this a little carefully.

                SampleMetaData theLastSample = null;
                IEnumerable<SampleMetaData> allBut = dataSamples;
                if (countOfEvents > 0 && countOfEvents > requestedNumberOfEvents)
                {
                    // Take up to the last one.
                    allBut = dataSamples.Take(dataSamples.Count() - 1);
                    theLastSample = dataSamples.Last();
                }

                var data1 = allBut
                    .SamplesAsSingleQueriable(avoidPlaces)
                    .AsBeamHaloStream(epoc)
                    .AsGoodJetStream(pTCut);

                var data = theLastSample == null ? data1
                    : data1.Concat(Files.GetSampleAsMetaData(theLastSample, avoidPlaces: avoidPlaces).AsBeamHaloStream(epoc).AsGoodJetStream(pTCut).Take(requestedNumberOfEvents - countOfEventsOneBack));

                // Check that we did ok. This will prevent errors down the line that are rather confusing.
                if (countOfEvents < requestedNumberOfEvents)
                {
                    Console.WriteLine($"Warning - unable to get all the events requested for {epoc.ToString()}. {countOfEvents} were found, and {requestedNumberOfEvents} events were requested.");
                }
                if (countOfEvents == 0 && requestedNumberOfEvents > 0)
                {
                    throw new InvalidOperationException($"Unable to get any events for {epoc.ToString()}!");
                }

                return data;
            }
            finally
            {
                // Reset the number of files we are looking at.
                Files.NFiles = oldNFiles;
            }
#endif
        }
    }
}
