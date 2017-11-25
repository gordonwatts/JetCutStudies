using DiVertAnalysis;
using libDataAccess.Utils;
using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// Include us in the MEF resolution so the various objects we are using are found during
        /// composition (e.g. the data scheme handlers).
        /// </summary>
        static Files()
        {
            TTreeQueryExecutor.AddAssemblyForPlugins(Assembly.GetCallingAssembly());
        }

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
        public static IQueryable<MetaData> GetSampleAsMetaData(string sample, bool weightByCrossSection = true, string[] avoidPlaces = null, int? nfiles = null)
        {
            // Options for the Uri for the grid dataset.
            var uriOptions = new Dictionary<string, string>();
            if (avoidPlaces != null && avoidPlaces.Length > 0)
            {
                var placesToAvoid = avoidPlaces?.Aggregate("", (acc, p) => acc + (acc.Length > 0 ? "," : "") + p);
                uriOptions["avoidPlaces"] = placesToAvoid;
            }
            var nf = nfiles.HasValue
                ? nfiles.Value
                : Files.NFiles;
            if (nf > 0)
            {
                uriOptions["nFiles"] = nf.ToString();
            }

            // Build the query tree.
            var backgroundFile = RecoverUri(sample, uriOptions);
            var backgroundEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(new[] { backgroundFile });
            backgroundEvents.UseStatementOptimizer = UseCodeOptimizer;
            backgroundEvents.IgnoreQueryCache = IgnoreQueires;

            // fetch the cross section weight so that we can re-weight this sample if need be.
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
        /// Build a URI for gridds with options.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static Uri RecoverUri(string s, IDictionary<string, string> options)
        {
            // Normalize the scope.
            var scope =
                s.Contains(":")
                ? s.Substring(0, s.IndexOf(":"))
                : s.Substring(0, s.IndexOf("."));
            if (s.Contains(":"))
            {
                s = s.Substring(s.IndexOf(":") + 1);
            }

            return new UriBuilder($"gridds://{scope}/{s}") { Query = options.ToOrderedQueryString() }.Uri;
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
        /// Return a sample for processing.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="weightByCrossSection"></param>
        /// <param name="avoidPlaces"></param>
        /// <returns></returns>
        public static IQueryable<MetaData> SamplesAsSingleQueriable(this IEnumerable<SampleMetaData> samples, bool weightByCrossSection = true, string[] avoidPlaces = null)
        {
            return samples
                .Select(s => GetSampleAsMetaData(s, weightByCrossSection, avoidPlaces))
                .Aggregate((acc, newSample) => acc.Concat(newSample));
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
        public static IQueryable<MetaData> GenerateStream(this IQueryable<recoTree> source, double xSecWeight)
        {
            return source.Select(e => new MetaData() { Data = e, xSectionWeight = xSecWeight * e.eventWeight });
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
