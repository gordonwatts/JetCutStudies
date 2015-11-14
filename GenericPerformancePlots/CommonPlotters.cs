using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static libDataAccess.PlotSpecifications;

namespace GenericPerformancePlots
{
    /// <summary>
    /// Some simple common plotters
    /// </summary>
    static class CommonPlotters
    {
        /// <summary>
        /// Make generic plots of pT, CalRatio, etc. for all jets
        /// </summary>
        /// <param name="jets">Source of jets</param>
        /// <param name="saveDir">Future directory where we will save these plots</param>
        /// <param name="nameAddition">Text to give to name and title of all plots we are making</param>
        public static IQueryable<DiVertAnalysis.recoTreeJets> PlotBasicDataPlots (this IQueryable<DiVertAnalysis.recoTreeJets> jets, FutureTDirectory saveDir, string nameAddition)
        {
            jets
                .FuturePlot(JetPtPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetEtaPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetCalRPlot, nameAddition)
                .Save(saveDir);

            return jets;
        }
    }
}
