using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    public static class UriExtensions
    {
        private static readonly Regex _regex = new Regex(@"[?|&]([\w\.]+)=([^?|^&]+)");

        /// <summary>
        /// Parse a query string into a dictionary w/out having to load up the web assembly.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, string> ParseQueryString(this Uri uri)
        {
            var match = _regex.Match(uri.PathAndQuery);
            var paramaters = new Dictionary<string, string>();
            while (match.Success)
            {
                paramaters.Add(match.Groups[1].Value, match.Groups[2].Value);
                match = match.NextMatch();
            }
            return paramaters;
        }
    }
}
