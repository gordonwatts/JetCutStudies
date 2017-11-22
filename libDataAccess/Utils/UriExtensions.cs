using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using libDataAccess.UriSchemeHandlers;

namespace libDataAccess.Utils
{
    /// <summary>
    /// Helper extensions mesthods for Uri parsing.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Parse the query string into key value pairs.
        /// </summary>
        private static readonly Regex _regex = new Regex(@"[?|&]([\w\.]+)=([^?|^&]+)");

        /// <summary>
        /// Parse a query string into a dictionary w/out having to load up the web assembly.
        /// </summary>
        /// <param name = "uri" ></ param >
        /// < returns ></ returns >
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

        /// <summary>
        /// Parse options in u and place them into the values of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="u"></param>
        /// <returns></returns>
        public static T ParseOptions<T>(this Uri u)
            where T : new ()
        {
            // Get a dict of options from u
            var dict = u.ParseQueryString();

            T v = new T();
            foreach (var k in dict)
            {
                var p = typeof(T).GetField(k.Key);
                if (p == null)
                {
                    throw new InvalidOperationException($"Attempted to set value {k.Key} on option type {typeof(T).Name}. Not valid!");
                }
                if (p.FieldType == typeof(int))
                {
                    p.SetValue(v, int.Parse(k.Value));
                }
                else if (p.FieldType == typeof(string))
                {
                    p.SetValue(v, k.Value);
                }
                else
                {
                    throw new InvalidOperationException($"Do not know how to parse type {p.FieldType.Name}!");
                }
            }

            return v;
        }

        /// <summary>
        /// Short hand to check if we cna parse the options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="u"></param>
        /// <returns></returns>
        public static bool CheckOptionsParse<T>(this Uri u)
            where T : new ()
        {
            try
            {
                var o = u.ParseOptions<T>();
                return true;
            } catch
            {
                return false;
            }
        }

        /// <summary>
        /// Build a query string from the object - only putting in values that are different from the
        /// ones that are default.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string BuildNonDefaultQuery<T>(T values)
            where T : new()
        {
            Dictionary<string, string> nameValueParis = new Dictionary<string, string>();
            var defaultOptions = new T ();
            foreach (var f in typeof(T).GetFields())
            {
                var dValue = f.GetValue(defaultOptions);
                var nValue = f.GetValue(values);

                if (!dValue.Equals(nValue))
                {
                    nameValueParis[f.Name] = nValue.ToString();
                }
            }

            return nameValueParis.ToOrderedQueryString();
        }

        /// <summary>
        /// Turn a dict in to a set of name value pairs for a query list.
        /// </summary>
        /// <param name="nameValues"></param>
        /// <returns></returns>
        public static string ToOrderedQueryString (this IDictionary<string, string> nameValues)
        {
            var qb = new StringBuilder();
            foreach (var p in nameValues)
            {
                if (qb.Length > 0)
                {
                    qb.Append("&");
                }
                qb.Append($"{p.Key}={p.Value}");
            }

            return qb.ToString();
        }
    }
}
