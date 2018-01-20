using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    public static class TaskUtils
    {
        public static Task<T[]> WhenAll<T> (this IEnumerable<Task<T>> source)
        {
            return Task.WhenAll(source);
        }
    }
}
