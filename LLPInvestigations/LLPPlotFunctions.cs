using DiVertAnalysis;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static libDataAccess.PlotSpecifications;

namespace LLPInvestigations
{
    static class LLPPlotFunctions
    {
        /// <summary>
        /// Make some basic plots for LLP's.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dir"></param>
        public static void PlotBasicLLPValues(this IQueryable<recoTreeLLPs> source, string name, FutureTDirectory dir)
        {
            source
                .FuturePlot(LLPPtPlot, name)
                .Save(dir);

            source
                .FuturePlot(LLPLxyPlot, name)
                .Save(dir);

            source
                .FuturePlot(LLPEtaPlot, name)
                .Save(dir);

            source
                .FuturePlot(LLPLzPlot, name)
                .Save(dir);

            source
                .FuturePlot(LLPLxyLzPlot, name)
                .Save(dir);
        }

        /// <summary>
        /// Plot some basic jet values
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="dir"></param>
        public static void PlotBasicValues(this IQueryable<recoTreeJets> source, string name, FutureTDirectory dir)
        {
            source
                .FuturePlot(JetPtPlot, name)
                .Save(dir);
            source
                .FuturePlot(JetEtaPlot, name)
                .Save(dir);
            source
                .FuturePlot(JetCalRPlot, name)
                .Save(dir);
        }
    }
}
