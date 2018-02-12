using libDataAccess.Utils;
using LinqToTTreeInterfacesLib;
using Polly;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace libDataAccess.UriSchemeHandlers
{
    /// <summary>
    /// We need to execute on a grid dataset.
    /// </summary>
    [Export(typeof(IDataFileSchemeHandler))]
    public class GridDSScheme : IDataFileSchemeHandler
    {
        /// <summary>
        /// Return the type of scheme we can deal with.
        /// </summary>
        public string Scheme => "gridds";

        /// <summary>
        /// What as the last modification date? Given that these items are on the grid, and thus never change
        /// (ha!) - we return a constant date.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public DateTime GetUriLastModificationDate(Uri u)
        {
            return new DateTime(1990, 12, 1);
        }

        /// <summary>
        /// Options allowed on our URL's.
        /// </summary>
        private class Options
        {
            /// <summary>
            ///  Number of files from each grid dataset we should take.
            /// </summary>
            public int nFiles = 0;

            /// <summary>
            /// The list of places to avoid using when we process this.
            /// </summary>
            [IgnoreAttributeForNormalization]
            public string avoidPlaces = "";

            [IgnoreAttributeForNormalization]
            public string preferPlaces = "";

            /// <summary>
            /// The job version
            /// </summary>
            public string jobName = "";

            /// <summary>
            /// The job name
            /// </summary>
            public int jobVersion = -1;
        }

        /// <summary>
        /// This is a good Uri if it has a DNS machinen, which is a scope, and a stemp, which is the name.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool GoodUri(Uri u)
        {
            return !string.IsNullOrWhiteSpace(u.DnsSafeHost)
                && u.Segments.Length == 2
                && u.CheckOptionsParse<Options>();
        }

        /// <summary>
        /// Our Uri is our normalized Uri.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Uri Normalize(Uri u)
        {
            // Order the options properly.
            var o = u.ParseOptions<Options>();

            // Write it out to a local file.
            WriteToLocalFile($"{u.DnsSafeHost}:{u.Segments[1]}", o);

            return new UriBuilder(u) { Query = UriExtensions.BuildNonDefaultQuery(o) }.Uri;
        }

        /// <summary>
        /// Turn the Uri into something we can process. We find all locations that we can,
        /// and then figure out what we should use as the location for them.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Uri>> ResolveUri(Uri u)
        {
            // Get out options
            var opt = u.ParseOptions<Options>();

            // There only a few scopes that really work - user isn't one of them, unfortunately. So, blank it out.
            // THis is definately a bit of a kludge to get around how we have stored some datasets by hand.
            var scope = u.DnsSafeHost == "user"
                ? ""
                : $"{u.DnsSafeHost}:";

            // Put together the scope and dsname so we can fetch the list of files.
            var ds_name = $"{scope}{u.Segments[1]}";
            var files = string.IsNullOrWhiteSpace(opt.jobName)
                ? await GetFileList(ds_name, nRequestedFiles: opt.nFiles,
                    avoidPlaces: string.IsNullOrWhiteSpace(opt.avoidPlaces) ? null : opt.avoidPlaces.Split(','),
                    preferPlaces: string.IsNullOrWhiteSpace(opt.preferPlaces) ? null : opt.preferPlaces.Split(','))
                : await GetFileList(ds_name, opt.jobName, opt.jobVersion, nRequestedFiles: opt.nFiles,
                    avoidPlaces: string.IsNullOrWhiteSpace(opt.avoidPlaces) ? null : opt.avoidPlaces.Split(','),
                    preferPlaces: string.IsNullOrWhiteSpace(opt.preferPlaces) ? null : opt.preferPlaces.Split(','));

            return files;
        }

        /// <summary>
        /// Where we are going ot cache the datasets we are looking at.
        /// </summary>
        const string datasets_needed = "datasets_needed.txt";

        /// <summary>
        /// Setup the class for first time running.
        /// </summary>
        static GridDSScheme()
        {
            // Remove the dataset log file if it is here.
            if (File.Exists(datasets_needed))
            {
                File.Delete(datasets_needed);
            }
        }
        
        /// <summary>
        /// Write out info about the files/datasets we are going after. We can uses these to make the files local.
        /// </summary>
        /// <param name="dataset_name"></param>
        /// <param name="opt"></param>
        private void WriteToLocalFile(string dataset_name, Options opt)
        {
            // There could be a few of us accessing this file for writing at once, so we need to protect against that.
            Policy
                .Handle<IOException>()
                .WaitAndRetryForever(count => TimeSpan.FromMilliseconds(100))
                .Execute(() => File.AppendAllText(datasets_needed, $"{opt.jobName} {opt.jobVersion} {opt.nFiles} {dataset_name}\r\n"));
        }

        /// <summary>
        /// Return a dataset list given the name of the dataset.
        /// </summary>
        /// <param name="dsname"></param>
        /// <returns></returns>
        private static Task<Uri[]> GetFileList(string dsname, string jobName, int jobNumber, string[] avoidPlaces = null, string[] preferPlaces = null, int nRequestedFiles = 0, bool verbose = false)
        {
            TraceListener listener = null;

            if (string.IsNullOrWhiteSpace(jobName))
            {
                throw new ArgumentException("jobName can't be empty");
            }

            if (verbose)
            {
                listener = new TextWriterTraceListener(Console.Out);
                Trace.Listeners.Add(listener);
            }

            try
            {
                return GRIDJobs.FindJobUris(jobName,
                    jobNumber,
                    dsname,
                    nRequestedFiles,
                    avoidPlaces: avoidPlaces,
                    preferPlaces: preferPlaces);
            }
            finally
            {
                if (listener != null)
                {
                    Trace.Listeners.Remove(listener);
                }
            }
        }

        /// <summary>
        /// Get a set of URI's for a dataset - no job number tranlsation.
        /// </summary>
        /// <param name="dsname"></param>
        /// <param name="avoidPlaces"></param>
        /// <param name="preferPlaces"></param>
        /// <param name="nRequestedFiles"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        private static Task<Uri[]> GetFileList(string dsname, string[] avoidPlaces = null, string[] preferPlaces = null, int nRequestedFiles = 0, bool verbose = false)
        {
            TraceListener listener = null;

            if (verbose)
            {
                listener = new TextWriterTraceListener(Console.Out);
                Trace.Listeners.Add(listener);
            }

            try
            {
                return GRIDJobs.FindDatasetUris(dsname,
                    nRequestedFiles,
                    avoidPlaces: avoidPlaces,
                    preferPlaces: preferPlaces);
            }
            finally
            {
                if (listener != null)
                {
                    Trace.Listeners.Remove(listener);
                }
            }
        }
    }
}
