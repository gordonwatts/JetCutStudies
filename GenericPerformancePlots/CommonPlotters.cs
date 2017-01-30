﻿using libDataAccess;
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
        public static IQueryable<JetInfoExtra> PlotBasicDataPlots (this IQueryable<JetInfoExtra> jets, FutureTDirectory saveDir, string nameAddition)
        {
            jets
                .FuturePlot(JetExtraPtPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetExtraEtaPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetExtraPhiPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetExtraCalRPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(NTrackExtraPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(TrackPtExtraPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetWidthPlotExtra, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(DeltaROfCloseTrackPlotExtra, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(BIBDeltaMinusTimingPlotExtra, nameAddition)
                .Save(saveDir);
            jets
                .FuturePlot(BIBDeltaPlusTimingPlotExtra, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(BIBMinusTimingPlotExtra, nameAddition)
                .Save(saveDir);
            jets
                .FuturePlot(BIBPlusTimingPlotExtra, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(BIBPlusTimingvsZExtra, nameAddition)
                .Save(saveDir);
            jets
                .FuturePlot(BIBMinusTimingvsZExtra, nameAddition)
                .Save(saveDir);

            // TODO: Running this loop takes waaay too long. What are we doing
            // wrong (if anything)?
            jets
                .FuturePlot(SumTrackPtPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(MaxTrackPtPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetExtraCalRVsPtPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetExtraNTrackVsPtPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetExtraCalRVsNTrackPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetExtraCalRVsMaxPtPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetExtraCalRVsSumPtPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetSumPtVsPtPlot, nameAddition)
                .Save(saveDir);

            jets
                .FuturePlot(JetMaxPtVsPtPlot, nameAddition)
                .Save(saveDir);


            return jets;
        }
    }
}
