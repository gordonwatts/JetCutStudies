using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    public static class Constants
    {
        /// <summary>
        /// The inner radius of the calorimeter EM wall (meters)
        /// </summary>
        /// <remarks>The proper setting of this can be seen by looking at the Lxy for JetMatched LLPs - it dies right at 4.0</remarks>
        public static double RadiusOfInnerEMWall = 1.8;

        /// <summary>
        /// The outter radius of the calorimeter HAD Wall. (meters)
        /// </summary>
        /// <remarks>The proper setting for this cut can be seen by looking at the CalR vs Lxy plot</remarks>
        public static double RadiusOfOutterHADWall = 4.0;

        /// <summary>
        /// Return an expression to test for something decaying in the calorimeter
        /// </summary>
        public static Expression<Func<double, bool>> InCalorimeter
        {
            get
            {
                return r => r >= RadiusOfInnerEMWall && r <= RadiusOfOutterHADWall;
            }
        }

        /// <summary>
        /// Regions of pT when we are splitting things up
        /// </summary>
        public static Tuple<double, double>[] PtRegions
            = new Tuple<double, double>[] {
                Tuple.Create(0.0, 25.0),
                Tuple.Create(25.0, 40.0),
                Tuple.Create(40.0, 60.0),
                Tuple.Create(60.0, 120.0),
                Tuple.Create(120.0, 200.0),
                Tuple.Create(200.0, 1000.0)
            };
    }
}
