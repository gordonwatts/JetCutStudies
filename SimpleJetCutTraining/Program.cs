using DiVertAnalysis;
using libDataAccess;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LinqToTTreeInterfacesLib;
using LINQToTTreeLib;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using TMVAUtilities;
using static LINQToTreeHelpers.ROOTUtils;
using static System.Console;
using static System.Math;

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
            var backgroundEvents = Files.GetJ2Z().Select(e => e.Data);
            //var signalHV125pi15Events = Files.Get125pi15();
            //var signalHV125pi40Events = Files.Get125pi40();
            var signalHV600pi100Events = Files.Get600pi150lt9m();
            //var signalGet200pi25lt5mEvents = Files.Get200pi25lt5m();

            //
            // Do a simple cut training here
            //

            var t = TrainingIQueriable(signalHV600pi100Events, true)
                .AsSignal("HV600pi100")
                .Background(TrainingIQueriable(backgroundEvents, false), "J2Z")
                .IgnoreVariables(p => p.lowestPtTrack);

            var mCuts = t.AddMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA.kCuts, "SimpleCuts")
                .Option("!H:V:FitMethod=MC:EffSel:SampleSize=200000:VarTransform=Decorrelate")
                .ParameterOption(p => p.logR, "VarProp", "FSmart")
                .ParameterOption(p => p.nTracks, "VarProp", "FSmart")
                .ParameterOption(p => p.lowestPtTrack, "VarProp", "FSmart");

            var mMVA = t.AddMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA.kBDT, "SimpleBDT")
                .Option("MaxDepth=3");

            t.Train("VerySimpleTraining");

            // Lets make a measurement of the efficiency for the standard cut.
            var effResults = new FutureTFile(Path.Combine(FileInfoUtilities.FindDirectoryWithFileMatching("*.sln").FullName,"trainingResults.root"));
            var stdCutVale = CalcEff(effResults.mkdir("std"), td => (td.logR > 1.2 && td.nTracks == 0) ? 1.0 : 0.0, 0.5, TrainingIQueriable(backgroundEvents, false), TrainingIQueriable(signalHV600pi100Events, true));

            // Next, do it with a reader

#if false
            var r = new ROOTNET.NTMVA.NReader();
            var s = new FileInfo("C:\\Users\\gordo\\Documents\\Code\\calratio2015\\JetCutStudies\\SimpleJetCutTraining\\bin\\x86\\Debug\\weights\\VerySimpleTraining_SimpleCuts.weights.xml");
            float[] logR = new float[2];
            float[] nTracks = new float[2];
            r.AddVariable("logR".AsTS(), logR);
            r.AddVariable("nTracks".AsTS(), nTracks);
            r.BookMVA("SimpleCuts".AsTS(), s.FullName.AsTS());
            Expression<Func<TrainingData, double>> simpleCutsReader = tv => TMVAReaders.TMVASelectorSimpleCutsTest(r, tv.logR, tv.lowestPtTrack, stdCutVale.Value);
#else
            Expression<Func<TrainingData, double>> simpleCutsReader = tv => TMVAReaders.TMVASelectorSimpleCuts(tv.logR, tv.nTracks, 0.72);
#endif
            var simpleCutValue = CalcEff(effResults.mkdir("SimpleCuts"), simpleCutsReader, 0.5, TrainingIQueriable(backgroundEvents, false), TrainingIQueriable(signalHV600pi100Events, true));

            // Next, we need to get the MVA and figure out the efficiency.
            Expression<Func<TrainingData, double>> simpleMVAReader = tv => TMVAReaders.TMVASelectorSimpleBDT(tv.logR, tv.nTracks);
            var simpleBDTValue = CalcEff(effResults.mkdir("SimpleBDT"), simpleMVAReader, 0.99999, TrainingIQueriable(backgroundEvents, false), TrainingIQueriable(signalHV600pi100Events, true));

            // Write out everything
            effResults.Write();
            effResults.Close();

            //Emit();
        }

        /// <summary>
        /// Do the cut
        /// </summary>
        /// <param name="effResults"></param>
        /// <param name="queryable1"></param>
        /// <param name="queryable2"></param>
        private static IFutureValue<double> CalcEff(FutureTDirectory effResults, Expression<Func<TrainingData,double>> selection, double threshold, IQueryable<TrainingData> background, IQueryable<TrainingData> signal)
        {
            background
                .Select(t => selection.Invoke(t))
                .FuturePlot("b_weight", "Background weight", 50, -1.1, 1.1)
                .Save(effResults);
            signal
                .Select(t => selection.Invoke(t))
                .FuturePlot("s_weight", "Signal weight", 50, -1.1, 1.1)
                .Save(effResults);

            var total_b = background.FutureCount();
            var total_s = signal.FutureCount();

            var selected_b = background.Where(t => selection.Invoke(t) > threshold).FutureCount();
            var selected_s = signal.Where(t => selection.Invoke(t) > threshold).FutureCount();

            var eff_b = from tb in total_b from ns in selected_b select (double) ns / (double) tb;
            var eff_s = from tb in total_s from ns in selected_s select (double) ns / (double) tb;

            //FutureWrite(from eb in eff_b from es in eff_s select $"Signal eff: {es}; Background eff: {eb}");

            return eff_s;
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
                    .Where(j => j.Jet.LLP.IsGoodIndex())
                    .Where(j => j.Jet.LLP.Lxy > 2 * 1000);
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
                                      nTracks = (int)j.Jet.nTrk,
                                      lowestPtTrack = trkList.First().pT,
                                  };
            return trainingDataSet;
        }
    }
}
