using DiVertAnalysis;
using libDataAccess;
using LINQToTTreeLib.Files;
using System.IO;
using System.Linq;
using LINQToTTreeLib;
using TMVAUtilities;
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
            // Do a simple cut training here
            //

            var t = TrainingIQueriable(signalHV600pi100Events, true)
                .AsSignal()
                .Background(TrainingIQueriable(backgroundEvents, false))
                .BookMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA.kCuts, "SimpleCuts", "V")
                .Train("VerySimpleTraining");
        }

        /// <summary>
        /// Filter the events correctly for signal or background and build the
        /// training data.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="isSignal"></param>
        /// <returns></returns>
        private static IQueryable<TrainingData> TrainingIQueriable(IQueryable<recoTree> events, bool isSignal)
        {
            // Look at all jets
            var trainingDataSetJets = events
                .SelectMany(e => e.Jets);

            // If this is to be treated as signal, then look for the LLP to have
            // decayed in the right way.
            if (isSignal)
            {
                trainingDataSetJets = trainingDataSetJets
                    .Where(j => j.LLP.IsGoodIndex());
            }

            // Fill our training data that will eventually be turned into the training tree.
            var trainingDataSet = trainingDataSetJets
                .Select(j => new TrainingData()
                {
                    logR = j.logRatio,
                    nTracks = (int)j.nTrk,
                });
            return trainingDataSet;
        }
    }
}
