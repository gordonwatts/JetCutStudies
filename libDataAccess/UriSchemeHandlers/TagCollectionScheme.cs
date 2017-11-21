using LinqToTTreeInterfacesLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace libDataAccess.UriSchemeHandlers
{
    /// <summary>
    /// Handle the tag collection scheme. Resolve from our csv file every dataset that matches a particular collections of tags
    /// (it performs an and).
    /// </summary>
    [Export(typeof(IDataFileSchemeHandler))]
    public class TagCollectionScheme : IDataFileSchemeHandler
    {
        /// <summary>
        /// Uri's that have the scheme tagcollection.
        /// </summary>
        public string Scheme => "tagcollection";

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
            return !string.IsNullOrWhiteSpace(u.DnsSafeHost);
        }

        /// <summary>
        /// Normalize the Uri. This means removing some options that aren't needed - but most importantly,
        /// adding a hash value for all the datasets this actually points to.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Uri Normalize(Uri u)
        {
            return u;
        }

        /// <summary>
        /// Resolve the Uri into its component parts.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public IEnumerable<Uri> ResolveUri(Uri u)
        {
            var raw_sample_names = SampleList(u);

            // Now, turn them into grid datasets.
            return raw_sample_names
                .Select(s => RecoverUri(s));
        }

        /// <summary>
        /// Given somethign from our csv file, convert it to a proper uri
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private Uri RecoverUri(string s)
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

            return new Uri($"gridds://{scope}/{s}");
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
