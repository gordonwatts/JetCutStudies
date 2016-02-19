using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    public static class ObjectUtils
    {
        /// <summary>
        /// If a test passes, execute a lambda.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="test"></param>
        /// <param name="whatToDo"></param>
        /// <returns></returns>
        public static T ExecuteIf<T> (this T obj, Func<T, bool> test, Action<T> whatToDo)
            where T : class
        {
            if (test(obj))
            {
                whatToDo(obj);
            }
            return obj;
        }

        /// <summary>
        /// If the object is null, run a small function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="whatToDo"></param>
        /// <returns></returns>
        public static T IfNull<T>( this T obj, Action<T> whatToDo)
            where T : class
        {
            return obj.ExecuteIf(o => o == null, whatToDo);
        }
    }
}
