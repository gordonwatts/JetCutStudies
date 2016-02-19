using DiVertAnalysis;
using libDataAccess;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Linq;
using System.Linq.Expressions;
using static libDataAccess.Utils.ROOTUtils;
using static System.Math;

namespace LLPInvestigations
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Finding the files");

#if false
            var llp125_15 = Files.Get125pi15();
            var llp125_40 = Files.Get125pi40();
            var llp600_100 = Files.Get600pi100();

            using (var outputHistograms = new FutureTFile("LLPInvestigations.root"))
            {
                // How often does a LLP get shared?

                ProcessSample(llp125_15, outputHistograms.mkdir("125-15"));
                ProcessSample(llp125_40, outputHistograms.mkdir("125-40"));
                ProcessSample(llp600_100, outputHistograms.mkdir("600-100"));
            }
#endif
        }

        private static void ProcessSample(QueriableTTree<DiVertAnalysis.recoTree> llp, FutureTDirectory dir)
        {
            var sharedJets = from ev in llp
                             from j1 in ev.Jets
                             from j2 in ev.Jets
                             where j1.LLP.IsGoodIndex() && j2.LLP.IsGoodIndex()
                             where j1 != j2
                             where j1.LLP == j2.LLP
                             select Tuple.Create(j1, j2);

            var count = sharedJets.FutureCount();

            // Calc how close things are for the LLP's
            var sharedLLPs = from ev in llp
                             let l1 = ev.LLPs.First()
                             let l2 = ev.LLPs.Skip(1).First()
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
                                  select from j in ev.Jets
                                         select from lp in ev.LLPs
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

            llpsCloseToJets
                .SelectMany(jets => jets)
                .Select(jets => jets.OrderByDescending(j => j.Item4).First())
                .FuturePlot("maxDPhiVsDRZoom", "Max DPhi between each jet and all LLPs in event vs DR; Delta Phi; Delta R",
                    60, 0, 0.4, jet => jet.Item4,
                    60, -0.5, 0.5, jet => Sqrt(jet.Item3))
                .Save(dir);

            var jetsCloseToLLPs = from ev in llp
                                  select from lp in ev.LLPs
                                         select from j in ev.Jets
                                                let dr = DR2.Invoke(j, lp)
                                                select Tuple.Create(j, lp, dr);

            jetsCloseToLLPs
                .SelectMany(jets => jets)
                .FuturePlot("maxDRForJets", "Max DR between each LLP and all Jets in event",
                    60, 0.0, 3.0, jets => Sqrt(jets.Max(v => v.Item3)))
                .Save(dir);

            // Dump some results
            Console.WriteLine($"Number of jets that share an LLP: {count.Value}");
        }
    }
}
