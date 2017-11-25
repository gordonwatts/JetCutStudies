﻿using libDataAccess.Utils;
using LinqToTTreeInterfacesLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            return new UriBuilder(u) { Query = UriExtensions.BuildNonDefaultQuery(o) }.Uri;
        }

        /// <summary>
        /// Turn the Uri into something we can process. We find all locations that we can,
        /// and then figure out what we should use as the location for them.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public IEnumerable<Uri> ResolveUri(Uri u)
        {
            // Get out options
            var opt = u.ParseOptions<Options>();

            // Put together the scope and dsname
            var ds_name = $"{u.DnsSafeHost}:{u.Segments[1]}";
            return GetFileList(ds_name, nRequestedFiles: opt.nFiles, avoidPlaces: string.IsNullOrWhiteSpace(opt.avoidPlaces) ? null : opt.avoidPlaces.Split(','));
        }

        /// <summary>
        /// Get/Set the job version number
        /// </summary>
        public static int JobVersionNumber = 201;

        /// <summary>
        /// Get/Set the job name we are fetching
        /// </summary>
        public static string JobName = "DiVertAnalysis";

        /// <summary>
        /// Return a dataset list given the name of the dataset.
        /// </summary>
        /// <param name="dsname"></param>
        /// <returns></returns>
        public static Uri[] GetFileList(string dsname, string[] avoidPlaces = null, int nRequestedFiles = 0, bool verbose = false)
        {
            TraceListener listener = null;

            if (verbose)
            {
                listener = new TextWriterTraceListener(Console.Out);
                Trace.Listeners.Add(listener);
            }

            try
            {
                return GRIDJobs.FindJobUris(JobName,
                    JobVersionNumber,
                    dsname,
                    nRequestedFiles,
                    avoidPlaces: avoidPlaces);
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
