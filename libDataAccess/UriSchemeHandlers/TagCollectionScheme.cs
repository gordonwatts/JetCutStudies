using LinqToTTreeInterfacesLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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
            return false;
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
            throw new NotImplementedException();
        }
    }
}
