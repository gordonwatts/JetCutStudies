using DiVertAnalysis;
using LINQToTTreeLib;
using System.Linq;
using System.Collections.Generic;
using static LINQToTreeHelpers.PlottingUtils;
using System.Linq.Expressions;
using System;

namespace libDataAccess
{
    // TODO: PlotStyle stuff needs comments so we can guess what it is doing.
    // TODO: MakePlotterSpec should have an example format string in the arguments
    // TODO: Add a logR text replacement
    // TOOD: What are all the low logR values in the all jets collection (they are less than -3 only in signal).

    // NOTE: The ntuple should be setup to use the Calibration Jet values only!

    /// <summary>
    /// Things to help with uniform binning of plots, etc.
    /// </summary>
    public class PlotSpecifications
    {
        /// <summary>
        /// The pT spectra we want.
        /// TODO: Fix so that other JetPtPlot folks depend on this, rather than repeating binning.
        /// </summary>
        public static IPlotSpec<double> JetPtPlotRaw =
            MakePlotterSpec<double>(50, 0.0, 300.0, j => j, "pT{0}", "pT of {0} jets; pT [GeV]");

        /// <summary>
        /// 1D plot of jet PT
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetPtPlot;

        /// <summary>
        /// 1D plot of jet PT.
        /// </summary>
        /// <remarks>Initialized below</remarks>
        public static IPlotSpec<JetInfoExtra> JetExtraPtPlot;

        /// <summary>
        /// 1D plot of jet eta
        /// </summary>
        public static IPlotSpec<double> JetEtaPlotRaw =
            MakePlotterSpec<double>(50, -5.0, 5.0, j => j, "eta{0}", "eta of {0} jets; eta");

        /// <summary>
        /// 1D plot of jet eta
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetEtaPlot;

        /// <summary>
        /// Plot the number of tracks.
        /// </summary>
        public static IPlotSpec<double> NTrackPlotRaw =
            MakePlotterSpec<double>(21, -0.5, 20.5, tks => tks, "ntracks{0}", "Number of tracks with {0}; N_tracks");

        /// <summary>
        /// Plot the number of tracks
        /// </summary>
        public static IPlotSpec<IEnumerable<recoTreeTracks>> NTrackPlot =
            MakePlotterSpec<IEnumerable<recoTreeTracks>> (21, -0.5, 20.5, tks => tks.Count(), "ntracks{0}", "Number of tracks with {0}; N_tracks");

        /// <summary>
        /// Plot the number of tracks.
        /// </summary>
        public static IPlotSpec<JetInfoExtra> NTrackExtraPlot;

        /// <summary>
        /// Plot the event weights
        /// </summary>
        public static IPlotSpec<double> TrainingEventWeight =
            MakePlotterSpec<double>(100, 1.0, -1.0, j => j, "weight{0}", "Event weight of {0}; Weight");

        /// <summary>
        /// A pT plot of tracks associated with jets
        /// </summary>
        public static IPlotSpec<recoTreeTracks> TrackPtPlot =
            MakePlotterSpec<recoTreeTracks>(200, 0.0, 20.0, t => t.pT, "trkPt{0}", "Track pT for {0} tracks; pT");

        /// <summary>
        /// Sum pT of all tracks
        /// </summary>
        public static IPlotSpec<double> SumTrackPtPlotRaw =
            MakePlotterSpec<double>(40, 0.0, 40.0, j => j, "sumTrkPt{0}", "Sum pT of tracks for {0}; Sum pT [GeV]");

        /// <summary>
        /// Sum pT of all tracks
        /// </summary>
        public static IPlotSpec<JetInfoExtra> SumTrackPtPlot =
            MakePlotterSpec<JetInfoExtra>(40, 0.0, 40.0, j => j.AllTracks.Sum(t => t.pT), "sumTrkPt{0}", "Sum pT of tracks for {0}; Sum pT [GeV]");

        /// <summary>
        /// Sum pT of all tracks
        /// </summary>
        public static IPlotSpec<double> MaxTrackPtPlotRaw =
            MakePlotterSpec<double>(40, 0.0, 20.0, j => j, "MaxTrkPt{0}", "Max pT of tracks for {0}; Max pT [GeV]");

        /// <summary>
        /// Calc the max pT of a collection of tracks
        /// </summary>
        public static Expression<Func<IEnumerable<recoTreeTracks>, double>> CalcMaxPt => tks => tks.Count() > 0 ? tks.OrderByDescending(t => t.pT).First().pT : 0.0;

        /// <summary>
        /// Sum pT of all tracks
        /// </summary>
        public static IPlotSpec<JetInfoExtra> MaxTrackPtPlot =
            MakePlotterSpec<JetInfoExtra>(40, 0.0, 20.0, j => CalcMaxPt.Invoke(j.AllTracks), "MaxTrkPt{0}", "Max pT of tracks for {0}; Max pT [GeV]");

        /// <summary>
        /// A pT plot of tracks associated with jets
        /// </summary>
        public static IPlotSpec<JetInfoExtra> TrackPtExtraPlot;

        /// <summary>
        /// 1D plot of jet eta
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraEtaPlot;

        /// <summary>
        /// Normalize the cal ratio rate
        /// </summary>
        public static Expression<Func<double, double>> NormalizeCalRatio
            = cr =>
            cr > 4 ? 3.99
            : cr < -3.0 ? -2.99
            : cr;

        /// <summary>
        /// 1D plot of cal ratio jets
        /// </summary>
        public static IPlotSpec<double> JetCalRPlotRaw =
            MakePlotterSpec<double>(50, -3.0, 4.0, j => NormalizeCalRatio.Invoke(j),
                "CalR{0}", "Log Ratio of {0} jets; logR");

        /// <summary>
        /// 1D plot of cal ratio jets
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetCalRPlot;

        /// <summary>
        /// 1D plot of cal ratio jets
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraCalRPlot;

        /// <summary>
        /// Plot CalRatio vs Lxy for jets
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetCalRVsLXYPlot =
            MakePlotterSpec<recoTreeJets>(25, -3.0, 4.0, j =>
                j.logRatio > 4 ? 3.99
                : j.logRatio < -3.0 ? -2.99
                : j.logRatio, 30, 0.0, 10.0, j => j.LLP.IsGoodIndex() ? j.LLP.Lxy / 1000 : 0.0,
                titleFormat: "CalRatio vs Lxy for {0}", nameFormat:"CalRvsLxy{0}"
            );

        /// <summary>
        /// Plot of CalR vs Jet Pt
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetCalRVsPtPlot =
            MakePlotterSpec<recoTreeJets>(25, -3.0, 4.0, j =>
                j.logRatio > 4 ? 3.99
                : j.logRatio < -3.0 ? -2.99
                : j.logRatio,
                30, 25.0, 1000.0, j => j.pT,
                titleFormat: "CalRatio vs Jet pT for {0}", nameFormat: "CalRvspT{0}"
            );

        /// <summary>
        /// Plot of CalR vs Jet Pt
        /// </summary>
        /// <remarks>
        /// Initialized below.
        /// </remarks>
        public static IPlotSpec<JetInfoExtra> JetExtraCalRVsPtPlot;

        /// <summary>
        /// Plot of CalR vs Ntrack
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraNTrackVsPtPlot =
            MakePlotterSpec<JetInfoExtra>(
                21, -0.5, 20.5, j => j.Tracks.Count(),
                30, 25.0, 1000.0, j => j.Jet.pT,
                titleFormat: "NTracks vs Jet pT for {0}", nameFormat: "NTrkvspT{0}"
            );

        /// <summary>
        /// Plot of Sum PT vs Jet Pt
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetSumPtVsPtPlot =
            MakePlotterSpec<JetInfoExtra>(40, 0.0, 40.0, j => j.AllTracks.Sum(t => t.pT),
                30, 25.0, 1000.0, j => j.Jet.pT,
                titleFormat: "Track Sum Pt vs Jet pT for {0}", nameFormat: "SumPtvspT{0}"
            );

        /// <summary>
        /// Plot of SumPT vs CalR
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraCalRVsSumPtPlot =
            MakePlotterSpec<JetInfoExtra>(25, -3.0, 4.0, j =>
                j.Jet.logRatio > 4 ? 3.99
                : j.Jet.logRatio < -3.0 ? -2.99
                : j.Jet.logRatio,
                40, 0, 40.0, j => j.AllTracks.Sum(t => t.pT),
                titleFormat: "CalRatio vs Track Sum pT for {0}", nameFormat: "CalRvsSumPt{0}"
            );

        /// <summary>
        /// Plot of MaxPT vs CalR
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraCalRVsMaxPtPlot =
            MakePlotterSpec<JetInfoExtra>(25, -3.0, 4.0, j =>
                j.Jet.logRatio > 4 ? 3.99
                : j.Jet.logRatio < -3.0 ? -2.99
                : j.Jet.logRatio,
                40, 0, 20.0, j => CalcMaxPt.Invoke(j.AllTracks),
                titleFormat: "CalRatio vs Max Track pT for {0}", nameFormat: "CalRvsMaxTrk{0}"
            );

        /// <summary>
        /// Plot of Sum PT vs Jet Pt
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetMaxPtVsPtPlot =
            MakePlotterSpec<JetInfoExtra>(40, 0, 20.0, j => CalcMaxPt.Invoke(j.AllTracks),
                30, 25.0, 1000.0, j => j.Jet.pT,
                titleFormat: "Track Max Pt vs Jet pT for {0}", nameFormat: "MaxPtvspT{0}"
            );

        /// <summary>
        /// Plot of CalR vs NTrack
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraCalRVsNTrackPlot =
            MakePlotterSpec<JetInfoExtra>(25, -3.0, 4.0, j =>
                j.Jet.logRatio > 4 ? 3.99
                : j.Jet.logRatio < -3.0 ? -2.99
                : j.Jet.logRatio,
                21, -0.5, 20.5, j => j.Tracks.Count(),
                titleFormat: "CalRatio vs Jet pT for {0}", nameFormat: "CalRvsNTrk{0}"
            );

        /// <summary>
        /// The 1D plot of the decay length for LLP's
        /// </summary>
        public static IPlotSpec<recoTreeLLPs> LLPLxyPlot =
            MakePlotterSpec<recoTreeLLPs>(50, 0.0, 10.0, llp => llp.Lxy / 1000, "LLPLxy{0}", "LLP Lxy for {0}; Lxy (m)");

        /// <summary>
        /// 1D plot of the eta for LLP's
        /// </summary>
        public static IPlotSpec<recoTreeLLPs> LLPEtaPlot =
            MakePlotterSpec<recoTreeLLPs>(50, -5.0, 5.0, llp => llp.eta, "LLPEta{0}", "LLP eta for {0}; eta");

        /// <summary>
        /// Get the dependencies right
        /// </summary>
        static PlotSpecifications()
        {
            JetPtPlot = JetPtPlotRaw.FromType<double, recoTreeJets>(j => j.pT);
            JetEtaPlot = JetEtaPlotRaw.FromType<double, recoTreeJets>(j => j.eta);
            JetCalRPlot = JetCalRPlotRaw.FromType<double, recoTreeJets>(j => j.logRatio);
            NTrackPlot = NTrackPlotRaw.FromType<double, IEnumerable<recoTreeTracks>>(tks => tks.Count());
            SumTrackPtPlot = SumTrackPtPlotRaw.FromType<double, JetInfoExtra>(j => j.AllTracks.Sum(t => t.pT));
            MaxTrackPtPlot = MaxTrackPtPlotRaw.FromType<double, JetInfoExtra>(j => CalcMaxPt.Invoke(j.AllTracks));

            JetExtraPtPlot = JetPtPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);
            JetExtraEtaPlot = JetEtaPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);
            JetExtraCalRPlot = JetCalRPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);
            NTrackExtraPlot = NTrackPlot.FromType<IEnumerable<recoTreeTracks>, JetInfoExtra>(jinfo => jinfo.Tracks);
            TrackPtExtraPlot = TrackPtPlot.FromManyOfType((JetInfoExtra j) => j.Tracks);
            JetExtraCalRVsPtPlot = JetCalRVsPtPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);
        }

    }
}
