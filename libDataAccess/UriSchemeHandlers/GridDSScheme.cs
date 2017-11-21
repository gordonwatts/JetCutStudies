using LinqToTTreeInterfacesLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
        /// This is a good Uri if it has a DNS machinen, which is a scope, and a stemp, which is the name.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool GoodUri(Uri u)
        {
            return !string.IsNullOrWhiteSpace(u.DnsSafeHost)
                && u.Segments.Length == 1;
        }

        /// <summary>
        /// Our Uri is our normalized Uri.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Uri Normalize(Uri u)
        {
            return u;
        }

        /// <summary>
        /// Turn the Uri into something we can process. We find all locations that we can,
        /// and then figure out what we should use as the location for them.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public IEnumerable<Uri> ResolveUri(Uri u)
        {
            // Put together the scope and dsname
            var ds_name = $"{u.DnsSafeHost}:{u.Segments[1]}";
            return Files.GetFileList(ds_name, nRequestedFiles: 0);
        }
    }
}
