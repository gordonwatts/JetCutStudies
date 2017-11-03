using DiVertAnalysis;
using LINQToTTreeLib;
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
        /// Get/Set the job version number
        /// </summary>
        public static int JobVersionNumber = 201;

        /// <summary>
        /// Get/Set the job name we are fetching
        /// </summary>
        public static string JobName = "DiVertAnalysis";


        [Serializable]
        public class DataSetHasNoFilesException : Exception
        {
            public DataSetHasNoFilesException() { }
            public DataSetHasNoFilesException(string message) : base(message) { }
            public DataSetHasNoFilesException(string message, Exception inner) : base(message, inner) { }
            protected DataSetHasNoFilesException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        /// <summary>
        /// Return a dataset list given the name of the dataset.
        /// </summary>
        /// <param name="dsname"></param>
        /// <returns></returns>
        private static Uri[] GetFileList(string dsname, string[] avoidPlaces = null)
        {
            TraceListener listener = null;

            if (VerboseFileFetch)
            {
                listener = new TextWriterTraceListener(Console.Out);
                Trace.Listeners.Add(listener);
            }

            try {
                //return GRIDJobs.FindJobFiles(JobName,
                //    JobVersionNumber,
                //    dsname,
                //    nFiles: NFiles,
                //    statusUpdate: l => Console.WriteLine(l),
                //    intelligentLocal: true);
                return GRIDJobs.FindJobUris(JobName,
                    JobVersionNumber,
                    dsname,
                    NFiles,
                    avoidPlaces: avoidPlaces);
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
        /// <param name="sample">Name of the sample we can find by doing the lookup in the CSV data file</param>
        /// <param name="weightByCrossSection">If true, pull x-section weights from the file, otherwise set them to be all 1.</param>
        /// <returns>A queriable that has the weights built in and the complete recoTree plus weights.</returns>
        public static IQueryable<MetaData> GetSampleAsMetaData(string sample, bool weightByCrossSection = true, string[] avoidPlaces = null)
        {
            // Build the query tree
            var backgroundFiles = GetFileList(sample, avoidPlaces);
            if (backgroundFiles.Length == 0)
            {
                throw new DataSetHasNoFilesException($"Dataset {sample} has no files - remove it from the input list!");
            }
            var backgroundEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFiles);
            backgroundEvents.UseStatementOptimizer = UseCodeOptimizer;
            backgroundEvents.IgnoreQueryCache = IgnoreQueires;

            // fetch the cross section weight
            double xSectionWeight = 1.0;
            if (weightByCrossSection)
            {
                try
                {
                    var sampleInfo = SampleMetaData.LoadFromCSV(sample);
                    var bkgEvents = backgroundEvents.Count();
                    xSectionWeight = bkgEvents == 0 ? 0 : (sampleInfo.FilterEfficiency * sampleInfo.CrossSection * Luminosity / backgroundEvents.Count());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"WARNING: Sample '{sample}' not found in x-section list. Assuming a cross section weight of 1.");
                    Console.WriteLine($"  Error: {e.Message}");
                }
            }

            // And return the stream.
            return GenerateStream(backgroundEvents, xSectionWeight);
        }

        /// <summary>
        /// Return the meta-data for a sample
        /// </summary>
        /// <param name="s"></param>
        /// <param name="weightByCrossSection">True if we should weight this sample by cross section or by 1</param>
        /// <returns></returns>
        public static IQueryable<MetaData> GetSampleAsMetaData(SampleMetaData s, bool weightByCrossSection = true, string[] avoidPlaces = null)
        {
            return GetSampleAsMetaData(s.Name, weightByCrossSection, avoidPlaces);
        }

        /// <summary>
        /// Given all the samples, return a single queriable.
        /// The formats are going to be the same, but we do have to split by location, unfortunately.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<MetaData> SamplesAsSingleQueriable(this IEnumerable<SampleMetaData> source)
        {
            // Get all the files into a single large sequence.
                var files = source
                .SelectMany(s => GetFileList(s.Name))
                .ToArray();

            var groupings = files
                .GroupBy(u => u.Scheme + u.Host);

            var events = groupings
                .Select(g =>
                {
                    var queriable = QueryablerecoTree.CreateQueriable(g.ToArray());
                    queriable.IgnoreQueryCache = IgnoreQueires;
                    return queriable;
                })
                .Aggregate<QueriableTTree<recoTree>, IQueryable<recoTree>>(null, (accum, gsource) => accum == null ? gsource : accum.Concat(gsource));

            return GenerateStream(events, 1.0);
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
            return SampleMetaData.AllSamplesWithTag("mc15c", "background", "jz")
                .Select(smd => GetSampleAsMetaData(smd.Name))
                .Aggregate((IQueryable<MetaData>)null, (s, add) => s == null ? add : s.Concat(add));
        }

        /// <summary>
        /// Return a metadata stream version of the event sequence, with appropriate event and sample weights applied.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="xSecWeight"></param>
        /// <returns></returns>
        private static IQueryable<MetaData> GenerateStream(this IQueryable<recoTree> source, double xSecWeight)
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

    static class FilesHelpers
    {
        static public Uri[] AsURIs (this IEnumerable<FileInfo> source, string protocal = "file")
        {
            return source
                .Select(f => new Uri($"{protocal}://{f.FullName}"))
                .ToArray();
        }
    }
}
