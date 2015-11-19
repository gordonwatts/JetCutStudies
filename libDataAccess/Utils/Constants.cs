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
        public static double RadiusOfInnerEMWall = 1.5;

        /// <summary>
        /// The outter radius of the calorimeter HAD Wall. (meters)
        /// </summary>
        public static double RadiusOfOutterHADWall = 1.8;

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
