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
    }
}
