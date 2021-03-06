﻿using CalRatioTMVAUtilities;
using CommandLine;
using JenkinsAccess;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTTreeLib;
using LINQToTTreeLib.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMVAUtilities;

namespace DumpTrainingInfo
{
    class Program
    {
        /// <summary>
        /// Custom parameters for running
        /// </summary>
        class Options : CommandLineUtils.CommonOptions
        {
            [Option("RunNumber", Required = true)]
            public int RunNumber { get; set; }

            [Option("EventNumber", Required = true)]
            public int EventNumber { get; set; }
        }
        /// <summary>
        /// Pass an event number and basic training information will be dumped.
        /// </summary>
        /// <param name="args"></param>
        static async Task Main(string[] args)
        {
            // Parse the arguments.
            var options = CommandLineUtils.ParseOptions<Options>(args);

            // Get all the samples we want to look at, and turn them into
            // jets with the proper weights attached for later use.

            var backgroundJets = await CommandLineUtils.GetRequestedBackground();

            //var allSources = new List<Tuple<string, IQueryable<Files.MetaData>>>() {
            //    Tuple.Create("600pi150lt9m", Files.Get600pi150lt9m().GenerateStream(1.0)),
            //    Tuple.Create("400pi100lt9m", Files.Get400pi100lt9m().GenerateStream(1.0)),
            //    Tuple.Create("200pi25lt5m", Files.Get200pi25lt5m().GenerateStream(1.0)),
            //};

            // The URL of the MVA we are going after
            var mvaUrl = new Uri("http://jenks-higgs.phys.washington.edu:8080/job/CalR-JetMVATraining/372/artifact/Jet.MVATraining-JetPt.CalRatio.NTracks.SumPtOfAllTracks.MaxTrackPt_BDT.weights.xml");

            // Get the weight file.
            var weights = ArtifactAccess.GetArtifactFile(mvaUrl).Result;

            // Now, create the MVA value from the weight file
            var mvaValue = MVAWeightFileUtils.MVAFromWeightFile<TrainingTree>(weights);

            // Go through the background jets and dump them.
            var js = backgroundJets
                .AsGoodJetStream()
                .FilterNonTrainingEvents();

            // Filter by event number and run number
            var jsGood = js;
            if (options.EventNumber != 0)
            {
                jsGood = jsGood.Where(j => j.EventNumber == options.EventNumber);
            }

            var jsWithTrainingInfo = from j in jsGood
                                     let trainingInfo = TrainingUtils.TrainingTreeConverter.Invoke(j)
                                     select new
                                     {
                                         RunNumber = j.RunNumber,
                                         EventNumber = j.EventNumber,
                                         TrainingInfo = trainingInfo,
                                         MVAValue = mvaValue.Invoke(TrainingUtils.TrainingTreeConverter.Invoke(j)),
                                     };

            // Dump the result to a CSV file
            var outputFile = jsWithTrainingInfo
                .AsCSV(new System.IO.FileInfo("TrainingInfo-Temp.csv"))
                .CombineCSVFiles(new FileInfo("TrainingInfo.csv"));
        }
    }
}
