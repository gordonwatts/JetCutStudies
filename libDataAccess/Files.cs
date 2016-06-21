using DiVertAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static libDataAccess.Utils.Constants;

namespace libDataAccess
{
    /// <summary>
    /// Centralize the files we are accessing so multiple programs don't have to keep re-writing stuff.
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// Default setting for # of files to fetch when we run. 0 means we are running on the full data sample.
        /// </summary>
        public static int NFiles = 1;

        /// <summary>
        /// Set to true if we should ignore all queries
        /// </summary>
        public static bool IgnoreQueires = false;

        /// <summary>
        /// Set to true to get a complete dump of what is going on during grid file access.
        /// </summary>
        public static bool VerboseFileFetch = false;

        /// <summary>
        /// Set when we want to use (or not use) the code optimizer when generating
        /// our C++. Usually only set for debugging the underlying library.
        /// </summary>
        public static bool UseCodeOptimizer = true;

        /// <summary>
        /// Return a dataset list given the name of the dataset.
        /// </summary>
        /// <param name="dsname"></param>
        /// <returns></returns>
        private static FileInfo[] GetFileList(string dsname)
        {
            TraceListener listener = null;

            if (VerboseFileFetch)
            {
                listener = new TextWriterTraceListener(Console.Out);
                Trace.Listeners.Add(listener);
            }

            try {
                return GRIDJobs.FindJobFiles("DiVertAnalysis",
                    6,
                    dsname,
                    nFiles: NFiles,
                    statusUpdate: l => Console.WriteLine(l),
                    intelligentLocal: true);
            } finally
            {
                if (listener != null)
                {
                    Trace.Listeners.Remove(listener);
                }
            }
        }

        /// <summary>
        /// Return the JZ sample as requested.
        /// </summary>
        /// <param name="jzIndex"></param>
        /// <returns></returns>
        public static IQueryable<MetaData> GetJZ(int jzIndex)
        {
            var sample = SampleMetaData.LoadFromCSV($"J{jzIndex}Z");
            return GetSampleAsMetaData(sample.Name);
        }

        /// <summary>
        /// Returns the sample as metadata, including an extract cross section weight.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static IQueryable<MetaData> GetSampleAsMetaData(string sample)
        {
            // Build the query tree
            var backgroundFiles = GetFileList(sample);
            var backgroundEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFiles);
            backgroundEvents.UseStatementOptimizer = UseCodeOptimizer;
            backgroundEvents.IgnoreQueryCache = IgnoreQueires;

            // fetch the cross section weight
            double xSectionWeight = 1.0;
            try
            {
                var sampleInfo = SampleMetaData.LoadFromCSV(sample);
                xSectionWeight = sampleInfo.FilterEfficiency * sampleInfo.CrossSection * Luminosity / backgroundEvents.Count();
                //Console.WriteLine($"Sample: {sample}");
                //Console.WriteLine($"  Total Weight: {xSectionWeight}");
                //Console.WriteLine($"  Number raw events: {backgroundEvents.Count()}");
            } catch (Exception e)
            {
                Console.WriteLine($"WARNING: Sample '{sample}' not found in x-section list. Assuming a cross section weight of 1.");
                Console.WriteLine($"  Error: {e.Message}");
            }

            // And return the stream.
            return GenerateStream(backgroundEvents, xSectionWeight);
        }

        /// <summary>
        /// Return the meta-data for a sample
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IQueryable<MetaData> GetSampleAsMetaData(SampleMetaData s)
        {
            return GetSampleAsMetaData(s.Name);
        }

        /// <summary>
        /// Metadata we hold for each sample
        /// </summary>
        public class MetaData
        {
            public recoTree Data;
            public double xSectionWeight;
        }

        /// <summary>
        /// Gets properly weighted background sample as one.
        /// </summary>
        /// <returns></returns>
        public static IQueryable<MetaData> GetAllJetSamples()
        {
            return
                GetJZ(2)
                .Concat(GetJZ(3))
                .Concat(GetJZ(4))
                .Concat(GetJZ(5))
                .Concat(GetJZ(6))
                .Concat(GetJZ(7))
                ;
        }

        /// <summary>
        /// Return a metadata stream version of the event sequence, with appropriate event and sample weights applied.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="xSecWeight"></param>
        /// <returns></returns>
        public static IQueryable<MetaData> GenerateStream(this IQueryable<recoTree> source, double xSecWeight)
        {
            return source.Select(e => new MetaData() { Data = e, xSectionWeight = xSecWeight * e.eventWeight });
        }

        public static IQueryable<recoTree> Get200pi25lt5m()
        {
            var sig = GetFileList("mc15_13TeV.304805.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH200_mS25_lt5m.merge.AOD.e4754_s2698_r7146_r6282");
            var sigEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(sig);
            sigEvents.IgnoreQueryCache = IgnoreQueires;
            sigEvents.UseStatementOptimizer = UseCodeOptimizer;
            return sigEvents;
        }

        public static IQueryable<recoTree> Get400pi100lt9m()
        {
            var sig = GetFileList("mc15_13TeV.304813.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH400_mS100_lt9m.merge.AOD.e4754_s2698_r7146_r6282");
            var sigEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(sig);
            sigEvents.UseStatementOptimizer = UseCodeOptimizer;
            sigEvents.IgnoreQueryCache = IgnoreQueires;
            return sigEvents;
        }

        public static IQueryable<recoTree> Get600pi150lt9m()
        {
            var sig = GetFileList("mc15_13TeV.304817.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH600_mS150_lt9m.merge.AOD.e4754_s2698_r7146_r6282");
            var sigEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(sig);
            sigEvents.UseStatementOptimizer = UseCodeOptimizer;
            sigEvents.IgnoreQueryCache = IgnoreQueires;
            return sigEvents;
        }
    }
}
