using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TMVAUtilities
{
    class ExpressionUtils
    {
        /// <summary>
        /// Given T, generate a lambda expression that calls the TMVA code directly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pnames"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Expression<Func<T, double>> BuildLambdaExpression<T>(List<string> pnames, TMVAReaderCodeGenerator<T> code)
        {
            var myTArgParam = Expression.Parameter(typeof(T));
            var args = pnames
                .Select(p => typeof(T).GetField(p))
                .Select(pf => Expression.Field(myTArgParam, pf))
                .Select(fa => (fa.Type != typeof(double)) ? Expression.Convert(fa, typeof(double)) as Expression : fa);

            var argTypes = pnames.Select(_ => typeof(double)).ToArray();
            var method = code.GetType().GetMethod("MVAResultValue", argTypes);
            var call = Expression.Call(Expression.Constant(code), method, args.ToArray());

            var lambda = Expression.Lambda<Func<T, double>>(call, myTArgParam);

            return lambda;
        }

        /// <summary>
        /// Given T, generate a lambda expression that calls the TMVA code directly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pnames"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static Expression<Func<T, float[]>> BuildMulticlassLambdaExpression<T>(List<string> pnames, TMVAReaderCodeGenerator<T> code)
        {
            var myTArgParam = Expression.Parameter(typeof(T));
            var args = pnames
                .Select(p => typeof(T).GetField(p))
                .Select(pf => Expression.Field(myTArgParam, pf))
                .Select(fa => (fa.Type != typeof(double)) ? Expression.Convert(fa, typeof(double)) as Expression : fa);

            var argTypes = pnames.Select(_ => typeof(double)).ToArray();
            var method = code.GetType().GetMethod("MVAMultiResultValue", argTypes);
            var call = Expression.Call(Expression.Constant(code), method, args.ToArray());

            var lambda = Expression.Lambda<Func<T, float[]>>(call, myTArgParam);

            return lambda;
        }
    }
}
