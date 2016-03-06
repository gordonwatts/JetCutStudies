﻿using LINQToTreeHelpers;
using LINQToTTreeLib;
using LINQToTreeHelpers.FutureUtils;
using System;
using System.Linq;
using System.Linq.Expressions;
using static JetMVATraining.SampleUtils;
using static libDataAccess.PlotSpecifications;
using static LINQToTreeHelpers.PlottingUtils;

namespace JetMVATraining
{
    /// <summary>
    /// The info we will hand to the training
    /// </summary>
    public class TrainingTree
    {
        public double Weight;
        public double JetPt;
        public double CalRatio;
        public double JetEta;
        public int NTracks;
        public double SumPtOfAllTracks;
        public double MaxTrackPt;
    }

    /// <summary>
    /// Utility functions to setup the training.
    /// </summary>
    public static class TrainingUtils
    {
        internal static IQueryable<TrainingTree> AsTrainingTree(this IQueryable<JetStream> source)
        {
            return source
                .Select(i => new TrainingTree()
                {
                    Weight = i.Weight,
                    CalRatio = NormalizeCalRatio.Invoke(i.JetInfo.Jet.logRatio),
                    JetPt = i.JetInfo.Jet.pT,
                    JetEta = i.JetInfo.Jet.eta,
                    NTracks = i.JetInfo.Tracks.Count(),
                    SumPtOfAllTracks = i.JetInfo.AllTracks.Sum(t => t.pT),
                    MaxTrackPt = CalcMaxPt.Invoke(i.JetInfo.AllTracks),
                });
        }

        private class PlotInfo
        {
            public IPlotSpec<double> Plotter;
            public Expression<Func<TrainingTree, double>> ValueGetter;
        }

        /// <summary>
        /// List of ways to plot all the training variables.
        /// </summary>
        private static PlotInfo[] _plotters = new PlotInfo[]
        {
            new PlotInfo() { Plotter = JetPtPlotRaw, ValueGetter = tu => tu.JetPt },
            new PlotInfo() { Plotter = JetEtaPlotRaw, ValueGetter = tu => tu.JetEta },
            new PlotInfo() { Plotter = JetCalRPlotRaw, ValueGetter = tu => tu.CalRatio },
            new PlotInfo() { Plotter = TrainingEventWeight, ValueGetter = tu => tu.Weight },
            new PlotInfo() { Plotter = NTrackPlotRaw, ValueGetter = tu => tu.NTracks },
            new PlotInfo() { Plotter = SumTrackPtPlotRaw, ValueGetter = tu => tu.SumPtOfAllTracks },
            new PlotInfo() { Plotter = MaxTrackPtPlotRaw, ValueGetter = tu => tu.MaxTrackPt },
        };

        /// <summary>
        /// Make plots of everything
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static IQueryable<TrainingTree> PlotTrainingVariables (this IQueryable<TrainingTree> source, FutureTDirectory dir, string tag)
        {
            // Plots of all the items
            foreach (var p in _plotters)
            {
                source
                    .Select(j => Tuple.Create(p.ValueGetter.Invoke(j), j.Weight))
                    .FuturePlot(p.Plotter.NameFormat, p.Plotter.TitleFormat, p.Plotter, tag)
                    .Save(dir);

                source
                    .Select(j => p.ValueGetter.Invoke(j))
                    .FuturePlot(p.Plotter, $"{tag}_unweighted")
                    .Save(dir);
            }

            // Continue on...
            return source;
        }
    }
}