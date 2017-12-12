using LinqToTTreeInterfacesLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using libDataAccess.Utils;
using System.Text;
using System.Security.Cryptography;

namespace libDataAccess.UriSchemeHandlers
{
    /// <summary>
    /// Handle the tag collection scheme. Resolve from our csv file every dataset that matches a particular collections of tags
    /// (it performs an and). Possible arguments:
    /// 
    ///     nFilesPerDataset = n // Max number of files per dataset that we point to. 0 means all files in the dataset, and is the default.
    /// </summary>
    [Export(typeof(IDataFileSchemeHandler))]
    public class TagCollectionScheme : IDataFileSchemeHandler
    {
        /// <summary>
        /// Uri's that have the scheme tagcollection.
        /// </summary>
        public string Scheme => "tagcollection";

        /// <summary>
        /// Various options that can be attached to the URL.
        /// </summary>
        private class Options
        {
#pragma warning disable CS0414
            // The number of files from each sample in the list we point to.
            public int nFilesPerSample = 0;

            [IgnoreAttributeForNormalization]
            public string avoidPlaces = "";

            // Hash kept in order to make sure the list of files we match are consistent.
            public string hash = "";

            /// <summary>
            /// Name of the job that produced these files
            /// </summary>
            public string jobName = "";

            /// <summary>
            /// Version of the job that produced these files.
            /// </summary>
            public int jobVersion = 0;
#pragma warning restore CS0414
        }

        /// <summary>
        /// We assume the date is constant - these aren't local files that are changing.
        /// The key will be that the datasets they point to remain constant - we will deal with that by encoding
        /// something in the normalization.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public DateTime GetUriLastModificationDate(Uri u)
        {
            return new DateTime(1990, 12, 1);
        }

        /// <summary>
        /// Check to see if the Uri is good. It must have at least one tag.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool GoodUri(Uri u)
        {
            // Make sure we can parse it and that we have at least one good tag.
            if (string.IsNullOrWhiteSpace(u.DnsSafeHost) 
                || !u.CheckOptionsParse<Options>())
            {
                return false;
            }

            // Make sure that we have at least one (or more) dataset names that come back from this
            // list of tags.
            return SampleList(u).Length > 0;
        }

        /// <summary>
        /// Normalize the Uri. This means removing some options that aren't needed - but most importantly,
        /// adding a hash value for all the datasets this actually points to.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Uri Normalize(Uri u)
        {
            // To make sure we don't get into trouble if the csv files changes with the data samples that
            // are actually part of this, we need to calculate a hash.
            var samples = SampleList(u).OrderBy(s => s).ToArray();
            var allSamples = samples
                .Aggregate(new StringBuilder(), (acc, s) => { acc.Append(s); return acc; })
                .ToString();

            string hashText;
            using (MD5 md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(allSamples));
                hashText = hash.Aggregate("", (acc, b) => acc + b.ToString());
            }

            // Get out the parameters, add the hash in.
            var o = u.ParseOptions<Options>();
            o.hash = hashText;

            // Return a new Uri with the various hashes built in.
            return new UriBuilder(u) { Query = UriExtensions.BuildNonDefaultQuery(o) }.Uri;
        }

        /// <summary>
        /// Resolve the Uri into its component parts.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public IEnumerable<Uri> ResolveUri(Uri u)
        {
            // Optiosn we hand off to everything.
            var opt = u.ParseOptions<Options>();
            var dopt = new Dictionary<string, string>();
            if (opt.nFilesPerSample != 0)
            {
                dopt["nFiles"] = opt.nFilesPerSample.ToString();
            }
            if (!string.IsNullOrWhiteSpace(opt.avoidPlaces))
            {
                dopt["avoidPlaces"] = opt.avoidPlaces;
            }
            if (!string.IsNullOrWhiteSpace(opt.jobName))
            {
                dopt["jobName"] = opt.jobName;
                dopt["jobVersion"] = opt.jobVersion.ToString();
            }

            // THe list of samples.
            var raw_sample_names = SampleList(u);

            // Now, turn them into grid datasets.
            return raw_sample_names
                .Select(s => RecoverUri(s, dopt));
        }

        /// <summary>
        /// Given somethign from our csv file, convert it to a proper uri
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private Uri RecoverUri(string s, IDictionary<string, string> optiongs)
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

            return new UriBuilder($"gridds://{scope}/{s}") { Query = optiongs.ToOrderedQueryString ()}.Uri;
        }

        /// <summary>
        /// Return a list of sample names.
        /// </summary>
        /// <param name="u">The tag list</param>
        /// <returns></returns>
        private string[] SampleList(Uri u)
        {
            // extract the tag list.
            var tags = new[] { u.DnsSafeHost }.Concat(u.Segments.Where(s => !string.IsNullOrWhiteSpace(s) && s != "/")).ToArray();
            return SampleMetaData.AllSamplesWithTag(tags)
                .Select(sd => sd.Name)
                .ToArray();
        }
    }
}
