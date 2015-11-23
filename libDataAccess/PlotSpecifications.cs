using DiVertAnalysis;
using LINQToTreeHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static LINQToTreeHelpers.PlottingUtils;

namespace libDataAccess
{
    // TODO: PlotStyle stuff needs comments so we can guess what it is doing.
    // TODO: MakePlotterSpec should have an example format string in the arguments
    // TODO: shoudl we be using the calib or the non-calib values?
    // TODO: Add a logR text replacement
    // TOOD: What are all the low logR values in the all jets collection (they are less than -3 only in signal).

    /// <summary>
    /// Things to help with uniform binning of plots, etc.
    /// </summary>
    public class PlotSpecifications
    {
        /// <summary>
        /// 1D plot of jet PT
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetPtPlot =
            MakePlotterSpec<recoTreeJets>(50, 0.0, 300.0, j => j.pT, "pT{0}", "pT of {0} jets; pT [GeV]");

        /// <summary>
        /// 1D plot of jet eta
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetEtaPlot =
            MakePlotterSpec<recoTreeJets>(50, -5.0, 5.0, j => j.eta, "eta{0}", "eta of {0} jets; eta");

        /// <summary>
        /// 1D plot of cal ratio jets
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetCalRPlot =
            MakePlotterSpec<recoTreeJets>(50, -3.0, 4.0, j => 
            j.logRatio > 4 ? 3.99 
            : j.logRatio < -3.0 ? -2.99
            : j.logRatio, 
                "CalR{0}", "Log Ratio of {0} jets; logR");

        /// <summary>
        /// The 1D plot of the decay length for LLP's
        /// </summary>
        public static IPlotSpec<recoTreeLLPs> LLPLxyPlot =
            MakePlotterSpec<recoTreeLLPs>(50, 0.0, 10.0, llp => llp.Lxy / 1000, "LLPLxy{0}", "LLP Lxy for {0}; Lxy (m)");
    }
}
