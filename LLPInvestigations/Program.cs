using DiVertAnalysis;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using LINQToTTreeLib.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static libDataAccess.Utils.FutureConsole;
using static libDataAccess.CutConstants;
using static LINQToTreeHelpers.ROOTUtils;
using static System.Math;
using System.IO;
using static libDataAccess.Utils.CommandLineUtils;

namespace LLPInvestigations
{
    class Program
    {
        /// <summary>
        /// For command line options unique to this program.
        /// </summary>
        class Options : CommonOptions
        {

        }
        static void Main(string[] args)
        {
            var opt = ParseOptions<Options>(args);

            var signalSources = SampleMetaData.AllSamplesWithTag("signal")
                .Select(info => Tuple.Create(info.NickName, Files.GetSampleAsMetaData(info)));

            using (var outputHistograms = new FutureTFile("LLPInvestigations.root"))
            {
                foreach (var s in signalSources)
                {
                    FutureWriteLine(s.Item1);
                    ProcessSample(s.Item2, outputHistograms.mkdir(s.Item1));
                }
            }

            FutureConsole.DumpToCout();
        }

        /// <summary>
        /// Make a series of plots for the LLP's.
        /// </summary>
        /// <param name="llp"></param>
        /// <param name="dir"></param>
        private static void ProcessSample(IQueryable<Files.MetaData> llp, FutureTDirectory dir)
        {
            // Dump basic plots with info for the LLPs.
            var llpStream = llp.SelectMany(l => l.Data.LLPs);
            llpStream
                .PlotBasicLLPValues("all", dir);

            // Next, for LLP's with a jet near them.
            var llpStreamWithJets = llp.SelectMany(ev => ev.Data.Jets)
                .Where(j => j.LLP.IsGoodIndex());
            llpStreamWithJets
                .Select(j => j.LLP)
                .PlotBasicLLPValues("withJet", dir);
            var llpStreamWithGoodJets = llpStreamWithJets
                .Where(j => j.ET > 40.0 && Math.Abs(j.eta) < JetEtaLimit);
            llpStreamWithGoodJets
                .Select(j => j.LLP)
                .PlotBasicLLPValues("withGoodJet", dir);
            llpStreamWithGoodJets
                .Where(j => Math.Abs(j.eta) <= 1.7)
                .PlotBasicValues("withGoodJetBarrel", dir);
            llpStreamWithGoodJets
                .Where(j => Math.Abs(j.eta) > 1.7)
                .PlotBasicValues("withGoodJetEndCap", dir);

            // Look at the number of times sharing occurs (should never happen)
            var sharedJets = from ev in llp
                             from j1 in ev.Data.Jets
                             from j2 in ev.Data.Jets
                             where j1.isGoodLLP && j2.isGoodLLP
                             where j1.LLP.IsGoodIndex() && j2.LLP.IsGoodIndex()
                             where j1 != j2
                             where j1.LLP == j2.LLP
                             select Tuple.Create(j1, j2);

            var count = sharedJets.FutureCount();

            FutureWriteLine(() => $"  Number of jets that share an LLP: {count.Value}");

            // Calculate how close things are for the LLP's
            var sharedLLPs = from ev in llp
                             let l1 = ev.Data.LLPs.First()
                             let l2 = ev.Data.LLPs.Skip(1).First()
                             select Tuple.Create(l1, l2);

            sharedLLPs
                .Select(l => Sqrt(DeltaR2(l.Item1.eta, l.Item1.phi, l.Item2.eta, l.Item2.phi)))
                .FuturePlot("DeltaRLLP", "The DeltaR between the two LLPs in the event", 20, 0.0, 3.0)
                .Save(dir);

            sharedLLPs
                .Select(l => DeltaPhi(l.Item1.phi, l.Item2.phi))
                .FuturePlot("DeltaPhiLLP", "The DeltaPhi between the two LLPs in the event", 60, 0.0, PI)
                .Save(dir);

            // How many LLPs are within 0.4 of a jet?
            Expression<Func<recoTreeJets, recoTreeLLPs, double>> DR2 = (l, j) => DeltaR2(l.eta, l.phi, j.eta, j.phi);
#if false
            double openingAngle = 0.4;
            var llpsCloseToJets = from ev in llp
                                  select from j in ev.Data.Jets
                                         where j.isGoodLLP
                                         select from lp in ev.Data.LLPs
                                                let dr = DR2.Invoke(j, lp)
                                                let dphi = Abs(DeltaPhi(j.phi, lp.phi))
                                                select Tuple.Create(j, lp, dr, dphi);

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
                .FuturePlot("maxDPhiForLLPs", "Max DPhi between each jet and all LLPs in event",
                    60, 0, PI, jets => jets.Max(v => v.Item4))
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
#endif

            // Look at jets that pass the Run 1 cuts but don't have an associated LLP.
            // Once we have the good and bad jets, partner them up with the closest LLP we can.
            var jetsOnTheirOwn = from ev in llp
                                 from j in ev.Data.Jets
                                 where j.isGoodLLP
                                 where Abs(j.eta) < JetEtaLimit && j.pT > 40.0
                                 where j.logRatio >= IsolationTrackPtCut
                                 where !j.LLP.IsGoodIndex()
                                 let closeLLP = ev.Data.LLPs.OrderBy(l => DR2.Invoke(j, l)).First()
                                 select Tuple.Create(j, closeLLP, Sqrt(DR2.Invoke(j, closeLLP)), ev.Data.eventNumber);

            var jetsWithPartner = from ev in llp
                                 from j in ev.Data.Jets
                                  where j.isGoodLLP
                                  where Abs(j.eta) < JetEtaLimit
                                  where j.logRatio >= IsolationTrackPtCut
                                 where j.LLP.IsGoodIndex()
                                 let closeLLP = ev.Data.LLPs.OrderBy(l => DR2.Invoke(j, l)).First()
                                 select Tuple.Create(j, closeLLP, Sqrt(DR2.Invoke(j, closeLLP)));

            jetsOnTheirOwn
                .Select(jinfo => jinfo.Item1)
                .PlotBasicValues("CalRNoLLPNear", dir);

            jetsWithPartner
                .Select(jinfo => jinfo.Item1)
                .PlotBasicValues("CalRLLPNear", dir);

            jetsOnTheirOwn
                .Select(jinfo => jinfo.Item3)
                .FuturePlot("DRNoLLPNear", "DR to nearest LLP for lonely CalR jets", 20, 0.0, 0.7)
                .Save(dir);

            jetsWithPartner
                .Select(jinfo => jinfo.Item3)
                .FuturePlot("DRLLPNear", "DR to nearest LLP for CalR jets with associated LLP", 20, 0.0, 0.7)
                .Save(dir);

            jetsOnTheirOwn
                .Select(jinfo => jinfo.Item2)
                .PlotBasicLLPValues("CalRNoLLPNear", dir);

            jetsWithPartner
                .Select(jinfo => jinfo.Item2)
                .PlotBasicLLPValues("CalRLLPNear", dir);

            jetsWithPartner
                .Where(jinfo => Math.Abs(jinfo.Item1.eta) <= 1.7)
                .Select(jinfo => jinfo.Item2)
                .PlotBasicLLPValues("CalRLLPNearBarrel", dir);

            jetsWithPartner
                .Where(jinfo => Math.Abs(jinfo.Item1.eta) > 1.7)
                .Select(jinfo => jinfo.Item2)
                .PlotBasicLLPValues("CalRLLPNearEndCap", dir);

            // Write out a small text file of the bad events so we can cross check.
            jetsOnTheirOwn
                .Select(i => new
                {
                    EventNumber = i.Item4,
                    DR = i.Item3,
                    JetEta = i.Item1.eta,
                    JetPhi = i.Item1.phi,
                    JetPt = i.Item1.pT,
                    LLPEta = i.Item2.eta,
                    LLPPhi = i.Item2.phi,
                    LLPPt = i.Item2.pT / 1000.0
                })
                .AsCSV(new FileInfo("lonlyevents.csv"));

            // And, finally, we need to count so we can have some efficiencies...
            var jetsWithPartnerCount = jetsWithPartner.FutureCount();
            var jetsOnTheirOwnCount = jetsOnTheirOwn.FutureCount();

            var fraction = from jOnOwn in jetsOnTheirOwnCount
                           from jPartner in jetsWithPartnerCount
                           select jOnOwn / ((double)jOnOwn + (double)jPartner);

            FutureConsole.FutureWriteLine(() => $"  Fraction of unpartnered jets: {fraction.Value}.");

#if false
            // Lets look at llp's matched to a jet next
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
            FutureWriteLine(() => $"  Number of events where two LLPs are closest to one jet: {eventsWithTwoMatchedJets.Value}");

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

            // The following causes a crash b.c. we aren't properly doing optimization, I think, in LINQToTTree
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
        }
    }
}
