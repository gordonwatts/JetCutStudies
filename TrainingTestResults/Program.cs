using JenkinsAccess;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TMVAUtilities;
using CalRatioTMVAUtilities;
using static libDataAccess.PlotSpecifications;

namespace TrainingTestResults
{
    class Program
    {
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

        /// <summary>
        /// Look at a number of MVA trainings and use them as results.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Parse the arguments.
            CommandLineUtils.Parse(args);

            // Get all the samples we want to look at, and turn them into
            // jets with the proper weights attached for later use.

            var backgroundJets = CommandLineUtils.GetRequestedBackground();
            var allBackgrounds = new List<Tuple<string, IQueryable<Files.MetaData>>>()
            {
                Tuple.Create("QCD", backgroundJets),
            };

            var allSources = new List<Tuple<string, IQueryable<Files.MetaData>>>() {
                Tuple.Create("600pi150lt9m", Files.Get600pi150lt9m().GenerateStream(1.0)),
                Tuple.Create("400pi100lt9m", Files.Get400pi100lt9m().GenerateStream(1.0)),
                Tuple.Create("200pi25lt5m", Files.Get200pi25lt5m().GenerateStream(1.0)),
            };

            // List the artifacts that we are going to be using.
            var mvaResults = new MVAInfo[]
            {
                new MVAInfo() { Name = "FirstPt", Artifact = new Uri("http://jenks-higgs.phys.washington.edu:8080/job/CalR-JetMVATraining/372/artifact/Jet.MVATraining-JetPt.CalRatio.NTracks.SumPtOfAllTracks.MaxTrackPt_BDT.weights.xml") },
                new MVAInfo() { Name = "FirstET", Artifact = new Uri("http://jenks-higgs.phys.washington.edu:8080/job/CalR-JetMVATraining/374/artifact/Jet.MVATraining-CalRatio.NTracks.SumPtOfAllTracks.MaxTrackPt.JetET_BDT.weights.xml") },
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
                        PlotMVAResult(s.Item2.AsGoodJetStream(), sampleD, mvaValue);
                    }

                    // And now we can make the plots for signal
                    foreach (var s in allSources)
                    {
                        var sampleD = d.mkdir(s.Item1);
                        PlotMVAResult(s.Item2.AsGoodJetStream().FilterLLPNear(), sampleD, mvaValue);
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
            source
                .Select(j => TrainingUtils.TrainingTreeConverter.Invoke(j))
                .Select(j => Tuple.Create(mvaValue.Invoke(j), j.Weight))
                .FuturePlot(TrainingEventWeight.NameFormat, TrainingEventWeight.TitleFormat, TrainingEventWeight, "ForJetsWithLLPNear")
                .Save(dir);
        }
    }
}
