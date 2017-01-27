using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    public static class ArrayUtils
    {
        /// <summary>
        /// Add a single item onto a stream of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="additional"></param>
        /// <returns></returns>
        public static IEnumerable<T> Add<T> (this IEnumerable<T> source, T additional)
        {
            return source.Concat(new T[] { additional });
        }
    }
}
