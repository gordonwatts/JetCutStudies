using DiVertAnalysis;
using LINQToTTreeLib;
using System;
using System.Linq.Expressions;

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
        /// Default DR for track association, squared.
        /// </summary>
        public static double TrackJetAssociationDR2 = 0.2 * 0.2;

        /// <summary>
        /// Default pT cut for tracks
        /// </summary>
        public static double TrackJetAssociationMinPt = 2.0;

        /// <summary>
        /// Default for assocaiting all tracks - like lots of them.
        /// </summary>
        public static double TrackJetAssociationAllMinPt = 0.200;

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
        /// Return an expression to test for an LLP to be decaying in the calorimeter.
        /// </summary>
        public static Expression<Func<recoTreeLLPs, bool>> LLPInCalorimeter
        {
            get
            {
                return llp => InCalorimeter.Invoke(llp.Lxy / 1000);
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

        /// <summary>
        /// Luminosity for the analysis, in inverse nanobarnes.
        /// 3,340,000
        /// </summary>
        public static readonly double Luminosity = 3.34 * 1.0e6;
    }
}
