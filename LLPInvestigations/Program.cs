using DiVertAnalysis;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Linq;
using System.Linq.Expressions;
using static System.Math;
using static LINQToTreeHelpers.ROOTUtils;
using libDataAccess;
using System.Collections.Generic;

namespace LLPInvestigations
{
    class Program
    {
        static void Main(string[] args)
        {

            var signalSources = new List<Tuple<string, IQueryable<Files.MetaData>>>() {
                Tuple.Create("600pi150lt9m", Files.Get600pi150lt9m().GenerateStream(1.0)),
                Tuple.Create("400pi100lt9m", Files.Get400pi100lt9m().GenerateStream(1.0)),
                Tuple.Create("200pi25lt5m", Files.Get200pi25lt5m().GenerateStream(1.0)),
            };

            using (var outputHistograms = new FutureTFile("LLPInvestigations.root"))
            {
                foreach (var s in signalSources)
                {
                    Console.WriteLine(s.Item1);
                    ProcessSample(s.Item2, outputHistograms.mkdir(s.Item1));
                }
            }
        }

        private static void ProcessSample(IQueryable<Files.MetaData> llp, FutureTDirectory dir)
        {
            // Look at the number of times sharing occurs (should never happen)
            var sharedJets = from ev in llp
                             from j1 in ev.Data.Jets
                             from j2 in ev.Data.Jets
                             where j1.LLP.IsGoodIndex() && j2.LLP.IsGoodIndex()
                             where j1 != j2
                             where j1.LLP == j2.LLP
                             select Tuple.Create(j1, j2);

            var count = sharedJets.FutureCount();

            // Calc how close things are for the LLP's
            var sharedLLPs = from ev in llp
                             let l1 = ev.Data.LLPs.First()
                             let l2 = ev.Data.LLPs.Skip(1).First()
                             select Tuple.Create(l1, l2);

            sharedLLPs
                .Select(l => Sqrt(DeltaR2(l.Item1.eta, l.Item1.phi, l.Item2.eta, l.Item2.phi)))
                .FuturePlot("DeltaRLLP", "The DeltaR between two LLP in the event", 20, 0.0, 3.0)
                .Save(dir);

            sharedLLPs
                .Select(l => DeltaPhi(l.Item1.phi, l.Item2.phi))
                .FuturePlot("DeltaPhiLLP", "The DeltaPhi between two LLP in the event", 60, 0.0, PI)
                .Save(dir);

            // How many LLPs are within 0.4 of a jet?
            Expression<Func<recoTreeJets, recoTreeLLPs, double>> DR2 = (l, j) => DeltaR2(l.eta, l.phi, j.eta, j.phi);
            double openingAngle = 0.4;
            var llpsCloseToJets = from ev in llp
                                  select from j in ev.Data.Jets
                                         select from lp in ev.Data.LLPs
                                                let dr = DR2.Invoke(j, lp)
                                                let dphi = Abs(DeltaPhi(j.phi, lp.phi))
                                                select Tuple.Create(j, lp, dr, dphi);

            llpsCloseToJets
                .SelectMany(jets => jets)
                .Select(jets => jets.Count())
                .FuturePlot("nLLPsPerJetCount", "Number of LLPs in each event with a jet", 5, 0.0, 5.0)
                .Save(dir);

            llpsCloseToJets
                .SelectMany(jets => jets)
                .Select(llps => llps.Where(tup => tup.Item3 < openingAngle * openingAngle).Count())
                .FuturePlot("nLLPsCloseToJet", $"Number of LLPs with DR < {openingAngle}", 5, 0.0, 5.0)
                .Save(dir);

            llpsCloseToJets
                .SelectMany(jets => jets)
                .FuturePlot("maxDRForLLPs", "Max DR between each Jet and all LLPs in event",
                    60, 0.0, 3.0, jets => Sqrt(jets.Max(v => v.Item3)))
                .Save(dir);

            llpsCloseToJets
                .SelectMany(jets => jets)
                .Where(j => j.First().Item1.logRatio > 1.2)
                .FuturePlot("maxDRForLLPsInCRJets", "Max DR between each CR Jet (logR>1.2) and all LLPs in event",
                    60, 0.0, 3.0, jets => Sqrt(jets.Max(v => v.Item3)))
                .Save(dir);

            llpsCloseToJets
                .SelectMany(jets => jets)
                .Where(j => j.First().Item1.logRatio > 1.2)
                .FuturePlot("maxDPhiForLLPsInCRJets", "Max DPhi between each CR Jet (logR>1.2) and all LLPs in event",
                    60, 0, PI, jets => jets.Max(v => v.Item4))
                .Save(dir);

            llpsCloseToJets
                .SelectMany(jets => jets)
                .FuturePlot("maxDPhiForLLPs", "Max DPhi between each jet and all LLPs in event",
                    60, 0, PI, jets => jets.Max(v => v.Item4))
                .Save(dir);

            llpsCloseToJets
                .SelectMany(jets => jets)
                .FuturePlot("maxDPhiForLLPsZoom", "Max DPhi between each jet and all LLPs in event",
                    60, 0, 0.4, jets => jets.Max(v => v.Item4))
                .Save(dir);

            llpsCloseToJets
                .SelectMany(jets => jets)
                .Select(jets => jets.OrderByDescending(j => j.Item4).First())
                .FuturePlot("maxDPhiVsDEtaZoom", "Max DPhi between each jet and all LLPs in event vs DEta; Delta Phi; Delta eta",
                    60, 0, 0.4, jet => jet.Item4,
                    60, -0.5, 0.5, jet => jet.Item1.eta - jet.Item2.eta)
                .Save(dir);

            var matchedLLPs = from ev in llp
                              where ev.Data.Jets.Count() >= 2
                              select from lp in ev.Data.LLPs
                                     let closeJet = (from j in ev.Data.Jets
                                                     let dr = DR2.Invoke(j, lp)
                                                     //where dr < 0.4*0.4
                                                     orderby dr ascending
                                                     select j).FirstOrDefault()
                                     where closeJet != null
                                     select Tuple.Create(lp, closeJet);

            var eventsWithTwoMatchedJets = matchedLLPs
                .Where(linfo => linfo.Count() == 2)
                .Where(linfo => linfo.First().Item2 == linfo.Skip(1).First().Item2)
                .FutureCount();

            matchedLLPs
                .SelectMany(linfo => linfo)
                .FuturePlot("DRLLPJet", "The DR between jet and LLP; DR", 100, 0.0, 1.6, linfo => DR2.Invoke(linfo.Item2, linfo.Item1))
                .Save(dir);

            matchedLLPs
                .SelectMany(linfo => linfo)
                .Where(linfo => linfo.Item1.Lxy > 2000.0)
                .FuturePlot("DRLLPJetInCal", "The DR between jet and LLP for Lxy > 2.0 m; DR", 100, 0.0, 1.6, linfo => DR2.Invoke(linfo.Item2, linfo.Item1))
                .Save(dir);


            matchedLLPs
                .SelectMany(linfo => linfo)
                .FuturePlot("DRvsCalRLLPJet", "The DR between jet and LLP; DR(jet,llp) ; CalRatio",
                    100, 0.0, 1.6, linfo => DR2.Invoke(linfo.Item2, linfo.Item1),
                    100, -2.0, 3.0, linfo => linfo.Item2.logRatio)
                .Save(dir);

            matchedLLPs
                .SelectMany(linfo => linfo)
                .Where(linfo => linfo.Item1.Lxy > 2000.0)
                .FuturePlot("DRvsCalRLLPJetInCal", "The DR between jet and LLP for Lxy > 2.0 m; DR(jet,llp) ; CalRatio",
                    100, 0.0, 1.6, linfo => DR2.Invoke(linfo.Item2, linfo.Item1),
                    100, -2.0, 3.0, linfo => linfo.Item2.logRatio)
                .Save(dir);

#if false
            var jetsCloseToLLPs = from ev in llp
                                  select from lp in ev.Data.LLPs
                                         select from j in ev.Data.Jets
                                                let dr = DR2.Invoke(j, lp)
                                                select Tuple.Create(j, lp, dr);

            jetsCloseToLLPs
                .SelectMany(jets => jets)
                .FuturePlot("maxDRForJets", "Max DR between each LLP and all Jets in event",
                    60, 0.0, 3.0, jets => Sqrt(jets.Max(v => v.Item3)))
                .Save(dir);

            llpsCloseToJets
                .SelectMany(jets => jets)
                .Select(jets => jets.OrderByDescending(j => j.Item4).First())
                .FuturePlot("maxDPhiVsDRZoom", "Max DPhi between each jet and all LLPs in event vs DR; Delta Phi; Delta R",
                    60, 0, 0.4, jet => jet.Item4,
                    60, -0.5, 0.5, jet => Sqrt(jet.Item3))
                .Save(dir);

#endif
            // Dump some results
            Console.WriteLine($"  Number of jets that share an LLP: {count.Value}");
            Console.WriteLine($"  Number of events where two LLPs are closest to one jet: {eventsWithTwoMatchedJets.Value}");
        }
    }
}
