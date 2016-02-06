using DiVertAnalysis;
using libDataAccess;
using LINQToTTreeLib.Files;
using System.IO;
using System.Linq;
using LINQToTTreeLib;
using static System.Console;

namespace SimpleJetCutTraining
{
    /// <summary>
    /// Do training on a per-jet basis, looking at some pretty simple variables like
    /// logR, nTrack, etc.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            WriteLine("Finding the files");
            var backgroundEvents = Files.GetJ2Z();
            //var signalHV125pi15Events = Files.Get125pi15();
            //var signalHV125pi40Events = Files.Get125pi40();
            var signalHV600pi100Events = Files.Get600pi100();

            //
            // Generate the output files.
            //

            WriteTrainingROOTFile(backgroundEvents, false, new FileInfo("J2Z.training.root"));
            WriteTrainingROOTFile(signalHV600pi100Events, true, new FileInfo("HV600pi100.training.root"));

            WriteLine($"Number of signalHV600pi100Events events: {signalHV600pi100Events.Count()}.");
        }

        /// <summary>
        /// Generate training ROOT file output
        /// </summary>
        /// <param name="events">List of events we should watch over</param>
        /// <param name="outputFile">Where to create an output ROOT file.</param>
        private static FileInfo WriteTrainingROOTFile(IQueryable<recoTree> events, bool isSignal, FileInfo outputFile)
        {
            var trainingDataSetJets = events
                .SelectMany(e => e.Jets);

            if (isSignal)
            {
                trainingDataSetJets = trainingDataSetJets
                    .Where(j => j.LLP.IsGoodIndex());
            }

            var trainingDataSet = trainingDataSetJets
                .Select(j => new TrainingData()
                {
                    logR = j.logRatio,
                    nTracks = (int)j.nTrk
                });

            trainingDataSet.AsTTree(outputFile);

            return outputFile;
        }
    }
}
