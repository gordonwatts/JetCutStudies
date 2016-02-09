using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TMVAUtilities
{
    static class ExpressionUtilities
    {
        /// <summary>
        /// Given a simple field access expression, return the name of the field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string ExtractField<T, U>(this Expression<Func<T, U>> f)
        {
            var l = f as LambdaExpression;
            var field = l.Body as System.Linq.Expressions.MemberExpression;
            return field.Member.Name;
        }
    }
}
