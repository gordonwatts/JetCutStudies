using LINQToTreeHelpers.FutureUtils;
using LinqToTTreeInterfacesLib;
using LINQToTTreeLib;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace libDataAccess.Utils
{
    public static class IQueriableUitils
    {

        /// <summary>
        /// Calculate the efficiency of a cut on a stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cut"></param>
        /// <param name="weight">The weight of each even is what is summed during the calculation</param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IFutureValue<double> CalcualteEfficiency<T>(this IQueryable<T> source, Expression<Func<T, bool>> cut, Expression<Func<T, double>> weight)
        {
            var totalWt = source.FutureAggregate(0.0, (sum, js) => sum + weight.Invoke(js));
            var passWt = source.Where(cut).FutureAggregate(0.0, (sum, js) => sum + weight.Invoke(js));
            return from tot in totalWt from pass in passWt select pass / tot;
        }
    }
}
