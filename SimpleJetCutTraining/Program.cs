using DiVertAnalysis;
using libDataAccess;
using LINQToTTreeLib;
using TMVAUtilities;
using System.Linq;
using static System.Console;
using static System.Math;
using static LINQToTreeHelpers.ROOTUtils;

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
                .AsSignal("HV600pi100")
                .Background(TrainingIQueriable(backgroundEvents, false), "J2Z");

            var mCuts = t.AddMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA.kCuts, "SimpleCuts")
                .Option("!H:V:FitMethod=MC:EffSel:SampleSize=20000:VarTransform=Decorrelate")
                .ParameterOption(p => p.logR, "VarProp", "FSmart")
                .ParameterOption(p => p.lowestPtTrack, "VarProp", "FSmart");

            t.Train("VerySimpleTraining");
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
#if false
            var allJets = from e in events
                          from j in e.Jets
                          where j.pT > 30.0 && Abs(j.eta) < 2.4
                          select new TrainingData()
                          {
                              logR = j.logRatio,
                              lowestPtTrack = (from t in e.Tracks
                                               where DeltaR2(t.eta, t.phi, j.eta, j.phi) < 0.2 * 0.2
                                               orderby t.pT ascending
                                               select t).First()

                          };
#endif
            // Look at all jets
            var trainingDataSetJets = events
                .SelectMany(e => e.Jets.Select(j => new { Jet = j, Event = e }))
                .Where(j => j.Jet.pT > 30.0)
                .Where(j => Abs(j.Jet.eta) < 2.4);

            // If this is to be treated as signal, then look for the LLP to have
            // decayed in the right way.
            if (isSignal)
            {
                trainingDataSetJets = trainingDataSetJets
                    .Where(j => j.Jet.LLP.IsGoodIndex());
            }

            // Fill our training data that will eventually be turned into the training tree.
            var trainingDataSet = from j in trainingDataSetJets
                                  let trkList = from t in j.Event.Tracks
                                                where DeltaR2(t.eta, t.phi, j.Jet.eta, j.Jet.phi) < 0.04
                                                orderby t.pT ascending
                                                select t
                                  where trkList.Any()
                                  select new TrainingData()
                                  {
                                      logR = j.Jet.logRatio,
                                      //nTracks = (int)j.Jet.nTrk,
                                      lowestPtTrack = trkList.First().pT,
                                  };
            return trainingDataSet;
        }
    }
}
