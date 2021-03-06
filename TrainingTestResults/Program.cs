﻿using JenkinsAccess;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CommandLine;
using TMVAUtilities;
using CalRatioTMVAUtilities;
using static libDataAccess.PlotSpecifications;
using static LINQToTreeHelpers.PlottingUtils;
using System.Threading.Tasks;

namespace TrainingTestResults
{
    public class Options : CommandLineUtils.CommonOptions
    {
        [Option("SignalTag", Default = "signal", HelpText = "Tag that will match samples treated as signal for the comparisons.")]
        public string SignalTag { get; set; }
    }
    class Program
    {
        /// <summary>
        /// Look at a number of MVA trainings and use them as results.
        /// </summary>
        /// <param name="args"></param>
        static async Task Main(string[] args)
        {
            // Parse the arguments.
            var opt = CommandLineUtils.ParseOptions<Options>(args);

            // Get all the samples we want to look at, and turn them into
            // jets with the proper weights attached for later use.

            var backgroundJets = await CommandLineUtils.GetRequestedBackground();
            var allBackgrounds = new List<Tuple<string, IQueryable<Files.MetaData>>>()
            {
                Tuple.Create("QCD", backgroundJets),
            };

            var allSources = await SampleMetaData.AllSamplesWithTag(opt.SignalTag)
                .Select(async info => Tuple.Create(info.NickName, await Files.GetSampleAsMetaData(info, false)))
                .WhenAll();
            if (allSources.Length == 0)
            {
                throw new ArgumentException($"No samples were found with tag '{opt.SignalTag}'.");
            }

            // List the artifacts that we are going to be using.
            var mvaResults = new MVAInfo[]
            {
                new MVAInfo() { Name = "13VariableLxy", Artifact = new Uri("http://higgs.phys.washington.edu:8080/job/CalRatio2016/job/JetMVATraining/38/artifact/Jet.MVATraining-Jet.CalRat.NTrac.SumPtOfAllTrac.MaxTrack.JetWid.JetDRTo2GeVTra.EnergyDensi.HadronicLayer1Fracti.JetL.JetLo.FirstClusterRadi.ShowerCent_BDT.weights.xml") },
                new MVAInfo() { Name = "13VariableLxyNoDR", Artifact = new Uri("http://higgs.phys.washington.edu:8080/job/CalRatio2016/job/JetMVATraining/39/artifact/Jet.MVATraining-Jet.CalRat.NTrac.SumPtOfAllTrac.MaxTrack.JetWid.EnergyDensi.HadronicLayer1Fracti.JetL.JetLo.FirstClusterRadi.ShowerCent_BDT.weights.xml") },
                new MVAInfo() { Name = "13VariableLxyNoDRNTrack", Artifact = new Uri("http://higgs.phys.washington.edu:8080/job/CalRatio2016/job/JetMVATraining/40/artifact/Jet.MVATraining-Jet.CalRat.SumPtOfAllTrac.MaxTrack.JetWid.EnergyDensi.HadronicLayer1Fracti.JetL.JetLo.FirstClusterRadi.ShowerCent_BDT.weights.xml") },
                new MVAInfo() { Name = "13VariableLxyNoNTrack", Artifact = new Uri("http://higgs.phys.washington.edu:8080/job/CalRatio2016/job/JetMVATraining/41/artifact/Jet.MVATraining-Jet.CalRat.NTrac.SumPtOfAllTrac.MaxTrack.JetWid.EnergyDensi.HadronicLayer1Fracti.JetL.JetLo.FirstClusterRadi.ShowerCent_BDT.weights.xml") },
            };

            // Fill an output file with the info for each MVA
            using (var f = new FutureTFile("TrainingTestResults.root"))
            {
                foreach (var mva in mvaResults)
                {
                    // The directory
                    var d = f.mkdir(mva.Name);

                    // Get the weight file.
                    var weights = ArtifactAccess.GetArtifactFile(mva.Artifact).Result;

                    // Now, create the MVA value from the weight file
                    var mvaValue = MVAWeightFileUtils.MVAFromWeightFile<TrainingTree>(weights);

                    // Do the backgrounds (e.g. ones where we don't filter for signal).
                    foreach (var s in allBackgrounds)
                    {
                        var sampleD = d.mkdir(s.Item1);
                        PlotMVAResult(s.Item2.AsGoodJetStream().FilterNonTrainingEvents(), sampleD, mvaValue);
                    }

                    // And now we can make the plots for signal
                    foreach (var s in allSources)
                    {
                        Console.WriteLine($"{mva.Name} - {s.Item1}");
                        var sampleD = d.mkdir(s.Item1);
                        PlotMVAResult(s.Item2.AsGoodJetStream().FilterNonTrainingEvents().FilterLLPNear(), sampleD, mvaValue);
                    }
                }
            }
        }

        /// <summary>
        /// Generate plots for the signal
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="sampleD"></param>
        /// <param name="mvaValue"></param>
        private static void PlotMVAResult(IQueryable<JetStream> source, FutureTDirectory dir, Expression<Func<TrainingTree, double>> mvaValue)
        {
            // Plot the weights. This can be used to plot signal vs background, ROC curves, etc.
            var weights = source
                .Select(j => TrainingUtils.TrainingTreeConverter.Invoke(j))
                .Select(j => Tuple.Create(mvaValue.Invoke(j), j.Weight))
                .FuturePlot(TrainingEventWeightFine.NameFormat, TrainingEventWeightFine.TitleFormat, TrainingEventWeightFine, "All")
                .Save(dir);

            // Next, let plot lots of kinematic plots so we can see what they look like.
            var plotsFromJS = new IPlotSpec<JetStream>[]
            {
                JetPtPlotJetStream,
                JetETPlotJetStream,
                JetEtaPlotJetStream,
                JetLLPLxyPlotJetStream,
                JetCalRPlotJetStream,
                JetStreamSumPt,
                JetStreamMaxPt,
                JetCalRPlotFineJetStream,
            };

            var plots = plotsFromJS
                .Select(myp => myp.FromType<JetStream, Tuple<JetStream, double>>(jinfo => jinfo.Item1, weight: jinfo => jinfo.Item2 * jinfo.Item1.Weight));

            // We want weighted and unweighted plots here. We first have to normalize the weighting to be from 0 to 1.
            // If there is only a single weight in the sample (which is just weird) then correctly make sure we are set to deal
            // with things.
            var firstNonZeroBinValue = weights.Value.FindNonZeroBinValue();
            var lastNonZeroBinValue = weights.Value.FindNonZeroBinValue(HistogramUtils.BinSearchOrder.HighestBin);

            if (firstNonZeroBinValue == lastNonZeroBinValue)
            {
                Console.WriteLine($"  Sample has events with all one weight ({firstNonZeroBinValue}).");
            }

            var scaleing = lastNonZeroBinValue == firstNonZeroBinValue
                ? 1.0 
                : 1.0 / (lastNonZeroBinValue - firstNonZeroBinValue);

            firstNonZeroBinValue = lastNonZeroBinValue == firstNonZeroBinValue
                ? firstNonZeroBinValue - 1.0
                : firstNonZeroBinValue;

            var mvaWeithedJetStream = source
                .Select(j => Tuple.Create(j, j.Weight * (mvaValue.Invoke(TrainingUtils.TrainingTreeConverter.Invoke(j)) - firstNonZeroBinValue)*scaleing));
            var weithedJetStream = source
                .Select(j => Tuple.Create(j, j.Weight));

            // And run through each plot
            foreach (var p in plots)
            {
                mvaWeithedJetStream
                    .FuturePlot(p, "MVA")
                    .Save(dir);
                weithedJetStream
                    .FuturePlot(p, "")
                    .Save(dir);
            }
        }
    }

    /// <summary>
    /// Helper class to carry along info about a MVA that we are running in our comparison.
    /// </summary>
    class MVAInfo
    {
        /// <summary>
        ///  The uri to the artifact for this mva
        /// </summary>
        public Uri Artifact;

        /// <summary>
        /// Short name we can use in plots, etc., for the mva.
        /// </summary>
        public string Name;
    }
}
