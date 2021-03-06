﻿using DiVertAnalysis;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static LINQToTreeHelpers.PlottingUtils;

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
    /// <remarks>
    /// You can't have auto-determined binning - as it might be run on multiple samples and combined!
    /// </remarks>
    public class PlotSpecifications
    {

        /// <summary>
        /// The pT spectra we want.
        /// TODO: Fix so that other JetPtPlot folks depend on this, rather than repeating binning.
        /// </summary>
        public static IPlotSpec<double> JetPtPlotRaw =
            MakePlotterSpec<double>(150, 0.0, 750.0, j => j, "pT{0}", "pT of {0} jets; pT [GeV]");
        public static IPlotSpec<recoTreeJets> JetPtPlot;
        public static IPlotSpec<JetStream> JetPtPlotJetStream;
        public static IPlotSpec<JetInfoExtra> JetExtraPtPlot;

        public static IPlotSpec<double> JetWidthPlotRaw =
            MakePlotterSpec<double>(100, 0.0, 0.3, j => j, "jetWidth{0}", "Width of {0} jets; DeltaR");
        public static IPlotSpec<recoTreeJets> JetWidthPlot;
        public static IPlotSpec<JetInfoExtra> JetWidthPlotExtra;

        /// <summary>
        /// Jet ET
        /// </summary>
        public static IPlotSpec<double> JetETPlotRaw =
            MakePlotterSpec<double>(150, 0.0, 750.0, j => j, "ET{0}", "ET of {0} jets; ET [GeV]");
        public static IPlotSpec<recoTreeJets> JetETPlot;
        public static IPlotSpec<JetStream> JetETPlotJetStream;

        /// <summary>
        /// 1D plot of jet eta
        /// </summary>
        public static IPlotSpec<double> JetEtaPlotRaw =
            MakePlotterSpec<double>(50, -5.0, 5.0, j => j, "eta{0}", "eta of {0} jets; eta");

        public static IPlotSpec<double> JetPhiPlotRaw =
            MakePlotterSpec<double>(50, -3.2, 3.2, j => j, "phi{0}", "phi of {0} jets; phi");

        /// <summary>
        /// 1D plot of jet eta
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetEtaPlot;
        public static IPlotSpec<JetStream> JetEtaPlotJetStream;

        public static IPlotSpec<recoTreeJets> JetPhiPlot;

        /// <summary>
        /// Plot of MaxPt
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraSumPt =
            MakePlotterSpec<JetInfoExtra>(40, 0.0, 40.0, j => j.AllTracks.Sum(t => t.pT),
                "SumPt{0}", "Track Sum Pt for {0}");
        public static IPlotSpec<JetStream> JetStreamSumPt;

        /// <summary>
        /// The max track pT
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraMaxPt =
            MakePlotterSpec<JetInfoExtra>(40, 0.0, 40.0, j => CalcMaxPt.Invoke(j.AllTracks),
                "SumPt{0}", "Track Sum Pt for {0}");
        public static IPlotSpec<JetStream> JetStreamMaxPt;

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
            MakePlotterSpec<double>(200, -10.0, 10.0, j => j, "weight{0}", "Event weight of {0}; Weight");

        public static IPlotSpec<double> TrainingEventWeightFine =
            MakePlotterSpec<double>(100000, -1.0, 1.0, j => j, "weight{0}", "Event weight of {0}; Weight");

        public static IPlotSpec<double> ClassifierEventWeight =
            MakePlotterSpec<double>(100, 0.0, 1.0, j => j, "weight{0}", "Event weight of {0}; Weight");

        /// <summary>
        /// A pT plot of tracks associated with jets
        /// </summary>
        public static IPlotSpec<recoTreeTracks> TrackPtPlot =
            MakePlotterSpec<recoTreeTracks>(200, 0.0, 20.0, t => t.pT, "trkPt{0}", "Track pT for {0} tracks; pT [GeV]");

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
        /// The DeltaR of the closest track that is at 2 GeV
        /// </summary>
        public static IPlotSpec<double> DeltaROfCloseTrackPlotRaw
            = MakePlotterSpec<double>(100, 0.0, 0.2, j => j, "DR2GeVTrack{0}", "DeltaR of 2 GeV track for {0}; DeltaR");
        public static IPlotSpec<JetInfoExtra> DeltaROfCloseTrackPlotExtra;
        public static Expression<Func<recoTreeTracks, recoTreeJets, double>> CalcDR2
            = (t, j) => ROOTUtils.DeltaR2(t.eta, t.phi, j.eta, j.phi);
        public static Expression<Func<IEnumerable<recoTreeTracks>, recoTreeJets, double>> CalcDR2GeVTrack
            = (tracks, jet) => 
            tracks.Where(t => t.pT > 2.0).Any() 
            ? Math.Sqrt(tracks.Where(t => t.pT > 2.0).Select(t => CalcDR2.Invoke(t, jet)).OrderBy(t => t).First())
            : 0.1999;

        /// <summary>
        /// The BIB timing - this is the predicted timing if the leading cell is actually coming from BIB rather than from the PV. If this matches
        /// the jet timing, then you are set.
        /// </summary>
        /// <remarks>
        /// Note that all length units coming from the tuple are mm, and time units are ns.
        /// </remarks>
        public static IPlotSpec<double> BIBTimingPlotRaw
            = MakePlotterSpec<double>(100, -25, 25, j => j, "BIBTiming{0}", "Timing for Lead Cluster for {0}; t [ns]");
        public static IPlotSpec<JetInfoExtra> BIBTimingPlotExtra;

        // (z-sqrt(z*z+3000*3000))/300
        public static IPlotSpec<double> BIBDeltaPlusTimingPlotRaw
            = MakePlotterSpec<double>(100, -25.0, 25.0, j => j, "BIBDeltaPlusTiming{0}", "BIB Delta Plus Timing for Lead Cluster for {0}; t [ns]");
        public static IPlotSpec<double> BIBDeltaMinusTimingPlotRaw
            = MakePlotterSpec<double>(100, -25.0, 25.0, j => j, "BIBDeltaMinusTiming{0}", "BIB Delta Minus Timing for Lead Cluster for {0}; t [ns]");
        public static IPlotSpec<JetInfoExtra> BIBDeltaPlusTimingPlotExtra;
        public static IPlotSpec<JetInfoExtra> BIBDeltaMinusTimingPlotExtra;

        public static Expression<Func<double, double, double>> CalcBIBTimingRaw
            = (z, lxy) => (z - Math.Sqrt(z * z + lxy * lxy)) / 3.0e11 * 1.0e9;
        public static Expression<Func<recoTreeJets, double>> CalcBIBPlusTiming
            = j => CalcBIBTimingRaw.Invoke(j.FirstClusterZ, j.FirstClusterLxy);
        public static Expression<Func<recoTreeJets, double>> CalcBIBMinusTiming
            = j => CalcBIBTimingRaw.Invoke(-j.FirstClusterZ, j.FirstClusterLxy);

        public static Expression<Func<recoTreeJets, double>> CalcBIBPlusDeltaPlotTiming
            = j => j.FIrstClusterTime - CalcBIBPlusTiming.Invoke(j);
        public static Expression<Func<recoTreeJets, double>> CalcBIBMinusDeltaPlotTiming
            = j => j.FIrstClusterTime - CalcBIBMinusTiming.Invoke(j);

        public static IPlotSpec<recoTreeJets> BIBTimingvsZ =
            MakePlotterSpec<recoTreeJets>(50, -5000, 5000, j => j.FirstClusterZ,
                50, -25, 25, j => j.FIrstClusterTime, titleFormat: "t cluster vs z for {0}; z [m]; t [ns]", nameFormat: "BIBTimingvsZ{0}");
        public static IPlotSpec<JetInfoExtra> BIBTimingvsZExtra;

        /// <summary>
        /// Plot the pileup event weight
        /// </summary>
        public static IPlotSpec<double> PileUpWeightRaw
            = MakePlotterSpec<double>(100, 0.0, 20.0, j => j, "pileupEventWeight{0}", "Pileup Event Weight {0}; Pileup Event Weight");
        public static IPlotSpec<recoTree> PileUpWeight;

        /// <summary>
        /// A pT plot of tracks associated with jets
        /// </summary>
        public static IPlotSpec<JetInfoExtra> TrackPtExtraPlot;

        /// <summary>
        /// 1D plot of jet eta
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraEtaPlot;
        public static IPlotSpec<JetInfoExtra> JetExtraPhiPlot;

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
        public static IPlotSpec<recoTreeJets> JetCalRPlot;
        public static IPlotSpec<JetStream> JetCalRPlotJetStream;

        public static IPlotSpec<double> JetCalRPlotFineRaw =
            MakePlotterSpec<double>(10000, -3.0, 4.0, j => NormalizeCalRatio.Invoke(j),
                "CalRF{0}", "Log Ratio of {0} jets (finely binned); logR");
        public static IPlotSpec<recoTreeJets> JetCalRPlotFine;
        public static IPlotSpec<JetStream> JetCalRPlotFineJetStream;

        /// <summary>
        /// Track the number of interactions per crossing
        /// </summary>
        public static IPlotSpec<double> EventInteractionsPerCossing =
            MakePlotterSpec<double>(100, 0.0, 60.0, j => j,
                "InteractionsPerCrossing{0}", "Number of Interactions Per Crossing for {0} jets");

        /// <summary>
        /// 1D plot of cal ratio jets
        /// </summary>
        public static IPlotSpec<JetInfoExtra> JetExtraCalRPlot;

        /// <summary>
        /// Predicted Lxy
        /// </summary>
        public static IPlotSpec<double> JetCalPredictedLxyPlotRaw =
            MakePlotterSpec<double>(100, 0.0, 6, lxy => lxy,
                "PredictedLxy{0}", "Predicted Lxy of {0} jets; Predicted Lxy [m]");
        public static IPlotSpec<JetStream> JetCalPredictedLxyPlot;
        public static IPlotSpec<JetInfoExtra> JetCalPredictedLxyPlotJetExtra;

        public static IPlotSpec<double> JetCalPredictedLzPlotRaw =
            MakePlotterSpec<double>(100, 0.0, 4, lxy => lxy,
                "PredictedLz{0}", "Predicted Lz of {0} jets; Predicted Lz [m]");
        public static IPlotSpec<JetStream> JetCalPredictedLzPlot;
        public static IPlotSpec<JetInfoExtra> JetCalPredictedLzPlotJetExtra;

        public static IPlotSpec<JetInfoExtra> JetCalPredictedLxyVsLxy =
            MakePlotterSpec<JetInfoExtra>(50, 0.0, 7.0, j => j.Jet.Predicted_Lxy / 1000.0,
                50, 0.0, 7.0, j => j.Jet.LLP.IsGoodIndex() ? j.Jet.LLP.Lxy / 1000.0 : 0.0,
                "PredictedLxyVsLxy{0}", "Predicted Lxy vs actual Lxy for {0} jets; Predicted Lxy [m]; Lxy [m]");

        public static IPlotSpec<JetInfoExtra> JetCalPredictedLzVsLz =
            MakePlotterSpec<JetInfoExtra>(50, 0.0, 10.0, j => j.Jet.Predicted_Lz / 1000.0,
                50, 0.0, 10.0, j => j.Jet.LLP.IsGoodIndex() ? j.Jet.LLP.Lz / 1000.0 : 0.0,
                "PredictedLzVsLz{0}", "Predicted Lz vs actual Lz for {0} jets; Predicted Lz [m]; Lz [m]");

        public static IPlotSpec<JetInfoExtra> JetCalPredictedLzVsPredictedLxy =
            MakePlotterSpec<JetInfoExtra>(50, 0.0, 10.0, j => j.Jet.Predicted_Lxy / 1000.0,
                50, 0.0, 10.0, j => j.Jet.Predicted_Lz / 1000.0,
                "PredictedLzVsPredictedLxy{0}", "Predicted Lxy vs predicted Lz for {0} jets; Predicted Lxy [m]; Lz [m]");

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
        /// 2D Lxy vs pT plot
        /// </summary>
        public static IPlotSpec<recoTreeJets> JetPtVsLXYPlot =
            MakePlotterSpec<recoTreeJets>(150, 0.0, 750.0, j => j.pT,
                30, 0.0, 10.0, j => j.LLP.IsGoodIndex() ? j.LLP.Lxy / 1000 : 0.0,
                titleFormat: "Jet pT vs Lxy for {0}", nameFormat: "pTvsLxy{0}"
            );

        /// <summary>
        /// Raw plotter for Lxy
        /// </summary>
        public static IPlotSpec<double> JetLLPLxyPlotRaw =
            MakePlotterSpec<double>(80, 0.0, 8.0, j => j, nFormat: "Lxy{0}", tFormat: "Lxy for {0}; Lxy [m]");
        public static IPlotSpec<recoTreeJets> JetLLPLxyPlot;
        public static IPlotSpec<JetStream> JetLLPLxyPlotJetStream;

        /// <summary>
        /// Raw plotter for Lz
        /// </summary>
        public static IPlotSpec<double> JetLLPLzPlotRaw =
            MakePlotterSpec<double>(80, 0.0, 6.0, j => j, nFormat: "Lz{0}", tFormat: "Lxy for {0}; Lxy [m]");
        public static IPlotSpec<recoTreeJets> JetLLPLzPlot;
        public static IPlotSpec<JetStream> JetLLPLzPlotJetStream;

        /// <summary>
        /// Plot the pT of the LLP
        /// </summary>
        public static IPlotSpec<double> JetLLPPtPlotRaw =
            MakePlotterSpec<double>(150, 0.0, 800, j => j, nFormat: "LLPPt{0}", tFormat: "LLP pT for {0}; pT [GeV]");
        public static IPlotSpec<recoTreeJets> JetLLPPtPlot;
        public static IPlotSpec<JetStream> JetLLPPtPlotJetStream;

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
        /// 1D plot of Lz
        /// </summary>
        public static IPlotSpec<recoTreeLLPs> LLPLzPlot =
            MakePlotterSpec<recoTreeLLPs>(50, 0.0, 10.0, llp => llp.Lz / 1000, "LLPLz{0}", "LLP Lz for {0}; Lz (m)");

        /// <summary>
        /// Scatter plot of the LLP guys
        /// </summary>
        public static IPlotSpec<recoTreeLLPs> LLPLxyLzPlot =
            MakePlotterSpec<recoTreeLLPs>(50, 0.0, 10.0, llp => llp.Lxy / 1000.0,
                50, 0.0, 10.0, llp => llp.Lz / 1000.0,
                "LLPLxyLz{0}", "LLP Lxy vs Lz for {0}; Lxy (m); Lz (m)");

        /// <summary>
        /// 1D plot of the eta for LLP's
        /// </summary>
        public static IPlotSpec<recoTreeLLPs> LLPEtaPlot =
            MakePlotterSpec<recoTreeLLPs>(50, -5.0, 5.0, llp => llp.eta, "LLPEta{0}", "LLP eta for {0}; eta");

        /// <summary>
        /// Plot LLP pt
        /// </summary>
        public static IPlotSpec<double> LLPPtPlotRaw =
            MakePlotterSpec<double>(150, 0.0, 750.0, j => j, "pT{0}", "pT of {0} LLP; pT [GeV]");
        public static IPlotSpec<recoTreeLLPs> LLPPtPlot;

        public static IPlotSpec<double> EnergyDensityPlotRaw =
            MakePlotterSpec<double>(100, 0.0, 1.0, j => j, "EnergyDensity{0}", "Energy Density of {0}; rho");

        public static IPlotSpec<double> HadronicL1FractPlotRaw =
            MakePlotterSpec<double>(100, -1.5, 1.5, j => j, "HadronicL1Frac{0}", "Fraction of energy in L1 (Had) of {0}; L1/(L1+L2+L3)");
        public static IPlotSpec<double> JetLatPlotRaw =
            MakePlotterSpec<double>(50, 0.0, 1.0, j => j, "JetLat{0}", "Jet Latitude of {0}");
        public static IPlotSpec<double> JetLongPlotRaw =
            MakePlotterSpec<double>(50, 0.0, 1.0, j => j, "JetLong{0}", "Jet Longiduninal of {0}");
        public static IPlotSpec<double> FirstClusterRadiusPlotRaw =
            MakePlotterSpec<double>(7*4, 0.0, 7.0, j => j/1000.0, "FirstClusterR{0}", "Radius of first cluster of {0}; R [m]");
        public static IPlotSpec<double> ShowerCenterPlotRaw =
            MakePlotterSpec<double>(50, 0.0, 10.0, j => j/1000.0, "ShowerCenter{0}", "Depth of shower center {0}; Lambda_center [m]");

        /// <summary>
        /// Get the dependencies right
        /// </summary>
        static PlotSpecifications()
        {
            JetPtPlot = JetPtPlotRaw.FromType<double, recoTreeJets>(j => j.pT);
            JetETPlot = JetETPlotRaw.FromType<double, recoTreeJets>(j => j.ET);
            JetWidthPlot = JetWidthPlotRaw.FromType<double, recoTreeJets>(j => j.width);
            JetEtaPlot = JetEtaPlotRaw.FromType<double, recoTreeJets>(j => j.eta);
            JetPhiPlot = JetPhiPlotRaw.FromType<double, recoTreeJets>(j => j.phi);

            JetLLPLxyPlot = JetLLPLxyPlotRaw.FromType<double, recoTreeJets>(j => j.LLP.IsGoodIndex() ? j.LLP.Lxy / 1000.0 : 0.0);
            JetLLPPtPlot = JetLLPPtPlotRaw.FromType<double, recoTreeJets>(j => j.LLP.IsGoodIndex() ? j.LLP.pT / 1000.0 : 0.0);
            JetLLPLzPlot = JetLLPLzPlotRaw.FromType<double, recoTreeJets>(j => j.LLP.IsGoodIndex() ? j.LLP.Lz / 1000.0 : 0.0);

            JetCalRPlot = JetCalRPlotRaw.FromType<double, recoTreeJets>(j => j.logRatio);
            JetCalRPlotFine = JetCalRPlotFineRaw.FromType<double, recoTreeJets>(j => j.logRatio);

            NTrackPlot = NTrackPlotRaw.FromType<double, IEnumerable<recoTreeTracks>>(tks => tks.Count());

            LLPPtPlot = LLPPtPlotRaw.FromType<double, recoTreeLLPs>(j => j.pT / 1000.0);

            JetPtPlotJetStream = JetPtPlot.FromType<recoTreeJets, JetStream>(j => j.JetInfo.Jet, weight: j => j.Weight);
            JetETPlotJetStream = JetETPlot.FromType<recoTreeJets, JetStream>(j => j.JetInfo.Jet, weight: j => j.Weight);
            JetEtaPlotJetStream = JetEtaPlot.FromType<recoTreeJets, JetStream>(j => j.JetInfo.Jet, weight: j=> j.Weight);
            JetLLPLxyPlotJetStream = JetLLPLxyPlot.FromType<recoTreeJets, JetStream>(j => j.JetInfo.Jet, weight: j => j.Weight);
            JetLLPPtPlotJetStream = JetLLPPtPlot.FromType<recoTreeJets, JetStream>(j => j.JetInfo.Jet, weight: j => j.Weight);
            JetCalRPlotJetStream = JetCalRPlot.FromType<recoTreeJets, JetStream>(j => j.JetInfo.Jet, weight: j => j.Weight);
            JetCalRPlotFineJetStream = JetCalRPlotFine.FromType<recoTreeJets, JetStream>(j => j.JetInfo.Jet, weight: j => j.Weight);
            JetStreamSumPt = JetExtraSumPt.FromType<JetInfoExtra, JetStream>(j => j.JetInfo, weight: j => j.Weight);
            JetStreamMaxPt = JetExtraMaxPt.FromType<JetInfoExtra, JetStream>(j => j.JetInfo, weight: j => j.Weight);

            SumTrackPtPlot = SumTrackPtPlotRaw.FromType<double, JetInfoExtra>(j => j.AllTracks.Sum(t => t.pT));
            MaxTrackPtPlot = MaxTrackPtPlotRaw.FromType<double, JetInfoExtra>(j => CalcMaxPt.Invoke(j.AllTracks));
            DeltaROfCloseTrackPlotExtra = DeltaROfCloseTrackPlotRaw.FromType<double, JetInfoExtra>(j => CalcDR2GeVTrack.Invoke(j.AllTracks, j.Jet));
            JetExtraPtPlot = JetPtPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);
            JetWidthPlotExtra = JetWidthPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);
            JetExtraEtaPlot = JetEtaPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);
            JetExtraPhiPlot = JetPhiPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);
            JetExtraCalRPlot = JetCalRPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);
            NTrackExtraPlot = NTrackPlot.FromType<IEnumerable<recoTreeTracks>, JetInfoExtra>(jinfo => jinfo.Tracks);
            TrackPtExtraPlot = TrackPtPlot.FromManyOfType((JetInfoExtra j) => j.Tracks);
            JetExtraCalRVsPtPlot = JetCalRVsPtPlot.FromType<recoTreeJets, JetInfoExtra>(jinfo => jinfo.Jet);

            JetCalPredictedLxyPlot = JetCalPredictedLxyPlotRaw.FromType<double, JetStream>(j => j.JetInfo.Jet.Predicted_Lxy / 1000.0);
            JetCalPredictedLzPlot = JetCalPredictedLzPlotRaw.FromType<double, JetStream>(j => j.JetInfo.Jet.Predicted_Lz / 1000.0);
            JetCalPredictedLxyPlotJetExtra = JetCalPredictedLxyPlotRaw.FromType<double, JetInfoExtra>(j => j.Jet.Predicted_Lxy / 1000.0);
            JetCalPredictedLzPlotJetExtra = JetCalPredictedLzPlotRaw.FromType<double, JetInfoExtra>(j => j.Jet.Predicted_Lz / 1000.0);

            PileUpWeight = PileUpWeightRaw.FromType<double, recoTree>(evt => evt.pileupEventWeight);

            BIBTimingPlotExtra = BIBTimingPlotRaw.FromType<double, JetInfoExtra>(j => j.Jet.FIrstClusterTime);
            BIBDeltaPlusTimingPlotExtra = BIBDeltaPlusTimingPlotRaw.FromType<double, JetInfoExtra>(j => CalcBIBPlusDeltaPlotTiming.Invoke(j.Jet));
            BIBDeltaMinusTimingPlotExtra = BIBDeltaMinusTimingPlotRaw.FromType<double, JetInfoExtra>(j => CalcBIBMinusDeltaPlotTiming.Invoke(j.Jet));
            BIBTimingvsZExtra = BIBTimingvsZ.FromType<recoTreeJets, JetInfoExtra>(j => j.Jet);
        }

    }
}
