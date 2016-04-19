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
            if (l.Body is MemberExpression)
            {
                var field = l.Body as MemberExpression;
                return field.Member.Name;
            } else if (l.Body.NodeType == ExpressionType.Convert && ((l.Body as UnaryExpression).Operand.NodeType == ExpressionType.MemberAccess))
            {
                var u = l.Body as UnaryExpression;
                var field = u.Operand as MemberExpression;
                return field.Member.Name;
            }
            else
            {
                throw new NotImplementedException($"Do not know how to convert an expression to a member reference: {l.Body.NodeType.ToString()}.");
            }
        }
    }
}
