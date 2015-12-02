using DiVertAnalysis;
using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LinqToTTreeInterfacesLib;
using LINQToTTreeLib;
using ROOTNET.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using static libDataAccess.GRIDJobs;
using static libDataAccess.JetInfoExtraHelpers;
using static libDataAccess.PlotSpecifications;
using static LINQToTreeHelpers.PlottingUtils;
using static System.Linq.Enumerable;

namespace GenericPerformancePlots
{
    class Program
    {
        /// <summary>
        /// Make generic plots of the signal or background
        /// </summary>
        /// <param name="args"></param>
        /// <remarks>
        /// TODO: Would studies of efficiencies here be better served by splitting into forward and central eta regions?
        /// TODO: What is going on with the jetPT?
        /// TODO: What is eta distribution of the jets that make it through, in particular with NTrack = 0?
        ///       It could be those are far forward and thus have no tracks.
        /// TODO: Should any of these plots look at stuff in the way that Heather has (2D heat maps for cuts)?
        /// </remarks>
        static void Main(string[] args)
        {
            Console.WriteLine("Finding the files");
            var backgroundEvents = Files.GetJ2Z();
            var signalHV125pi15Events = Files.Get125pi15();
            var signalHV125pi40Events = Files.Get125pi40();
            var signalHV600pi100Events = Files.Get600pi100();
            signalHV125pi15Events.CleanupQuery = false;
            signalHV125pi15Events.IgnoreQueryCache = true;
            signalHV125pi15Events.UseStatementOptimizer = true;

            // Output file
            Console.WriteLine("Opening outupt file");
            using (var outputHistograms = new FutureTFile("GenericPerformancePlots.root"))
            {
                var status125pi15  = PerSampleStudies(backgroundEvents, signalHV125pi15Events, outputHistograms.mkdir("125-15"));
                var status125pi40  = PerSampleStudies(backgroundEvents, signalHV125pi40Events, outputHistograms.mkdir("125-40"));
                var status600pi100 = PerSampleStudies(backgroundEvents, signalHV600pi100Events, outputHistograms.mkdir("600-100"));

                var sigAll = signalHV125pi15Events
                    .Concat(signalHV125pi40Events)
                    .Concat(signalHV600pi100Events);

                //var statusAll = PerSampleStudies(backgroundEvents, sigAll, outputHistograms.mkdir("all"));

                // Run everything
                outputHistograms.Write();

                // Let the world know what is up
                DumpResults("125-15:", status125pi15);
                DumpResults("125-40:", status125pi40);
                DumpResults("600-100:", status600pi100);
                //Console.WriteLine($"all:      {statusAll.Value}");

            }
        }

        /// <summary>
        /// Simple dump aid function.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="strings"></param>
        private static void DumpResults(string title, List<IFutureValue<string>> strings)
        {
            Console.WriteLine(title);
            foreach (var l in strings)
            {
                Console.WriteLine($"  {l.Value}");
            }
        }

        /// <summary>
        /// Allow for studying multiple samples
        /// </summary>
        /// <param name="background"></param>
        /// <param name="signal"></param>
        /// <param name="outputHistograms"></param>
        /// <returns></returns>
        private static List<IFutureValue<string>> PerSampleStudies(IQueryable<recoTree> background, IQueryable<recoTree> signal, FutureTDirectory outputHistograms)
        {
            var backgroundJets = BuildSuperJetInfo(background);
            var signalJets = BuildSuperJetInfo(signal);

            backgroundJets
                .PlotBasicDataPlots(outputHistograms.mkdir("background"), "all");
            signalJets
                .PlotBasicDataPlots(outputHistograms.mkdir("signal"), "all");

            // Some basic info about the LLP's
            LLPBasicInfo(signal.SelectMany(s => s.LLPs), signal.SelectMany(s => s.Jets), outputHistograms.mkdir("signalLLP"));

            // Cal efficiency plots for CalR
            CalcSignalToBackgroundSeries(
                signalJets,
                backgroundJets,
                JetExtraCalRPlot,
                outputHistograms.mkdir("sigrtbackCalR"),
                "CalR");

            // Cal efficiency plots for NTrack
            CalcSignalToBackgroundSeries(
                signalJets,
                backgroundJets,
                NTrackExtraPlot,
                outputHistograms.mkdir("sigrtbackNTrk"),
                "Ntrk");

            // Next, as a function of pT
            var backRejValues = new double[] { 0.999, 0.99, 0.95, 0.9 };
            var sigValues = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 };
            var result = new List<IFutureValue<string>>();
            foreach (var ptRegion in Constants.PtRegions)
            {
                var dir = outputHistograms.mkdir($"sigrtback_{ptRegion.Item1}_{ptRegion.Item2}");

                var signalGoodJets = signalJets
                    .Where(j => j.Jet.pT >= ptRegion.Item1 && j.Jet.pT < ptRegion.Item2);

                var backgroundGoodJets = backgroundJets
                    .Where(j => j.Jet.pT >= ptRegion.Item1 && j.Jet.pT < ptRegion.Item2);

                var sigBackCalR = CalcSignalToBackgroundSeries(
                    signalGoodJets,
                    backgroundGoodJets,
                    JetExtraCalRPlot,
                    dir,
                    "CalR");

                var sigBackNtrk = CalcSignalToBackgroundSeries(
                    signalGoodJets,
                    backgroundGoodJets,
                    NTrackExtraPlot,
                    dir,
                    "Ntrk");

                var requiredBackValues = from bv in backRejValues
                                         select from bHist in sigBackCalR.Item2
                                                from sHist in sigBackCalR.Item1
                                                let backCut = CalcEffValue(bHist, bv)
                                                let effValue = LookupEffAtCut(sHist, backCut)
                                                select $"{ptRegion.Item1}: Cut for Back Rejection of {bv} is {backCut} (sig eff is {effValue})";
                result.AddRange(requiredBackValues);

                var requiredSigValues = from sv in sigValues
                                        select from bHist in sigBackCalR.Item2
                                               from sHist in sigBackCalR.Item1
                                               let sigCut = CalcEffValue(sHist, sv, false)
                                               let backRejection = LookupEffAtCut(bHist, sigCut)
                                               select $"{ptRegion.Item1}: Cut for Sig Eff of {sv} is {sigCut} (back rej is {backRejection})";
                result.AddRange(requiredSigValues);
            }

            // Do a simple calc for info reasons which we will type out.
            var status = from nB in background.FutureCount()
                         from nS in signal.FutureCount()
                         select string.Format("Signal events: {0} Background events: {1}", nB, nS);
            result.Add(status);
            return result;
        }

        /// <summary>
        /// Given a cut value (x axis value), return the value of the histo at that point.
        /// </summary>
        /// <param name="hist"></param>
        /// <param name="xAxisValue"></param>
        /// <returns></returns>
        private static double LookupEffAtCut(NTH1 hist, double xAxisValue)
        {
            var bin = hist.Xaxis.FindBin(xAxisValue);
            return hist.GetBinContent(bin);
        }

        /// <summary>
        /// Return the x-axis value for a partiuclar efficiency.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="bv"></param>
        /// <returns></returns>
        private static double CalcEffValue(NTH1 r, double bv, bool greater = true)
        {
            var firstValue = Enumerable.Range(0, r.NbinsX)
                .Where(bin => greater ? r.GetBinContent(bin) > bv : r.GetBinContent(bin) < bv)
                .FirstOrDefault();

            return r.Xaxis.GetBinCenter(firstValue);
        }

        /// <summary>
        /// Some very basic LLP plots. Good LLPs are < 1.7 in eta?? For forward it is 1.7 to 2.5.
        /// </summary>
        /// <param name="LLPsToPlot"></param>
        /// <param name="dir"></param>
        private static void LLPBasicInfo(IQueryable<recoTreeLLPs> LLPsToPlot, IQueryable<recoTreeJets> jets, FutureTDirectory dir)
        {
            // LLP's and LLP's assocated with a jet
            LLPsToPlot
                .FuturePlot(LLPLxyPlot, "all")
                .Save(dir);

            LLPsToPlot
                .FuturePlot(LLPEtaPlot, "all")
                .Save(dir);

            var jetsWithLLPS = jets
                .Where(j => j.LLP.IsGoodIndex());

            jetsWithLLPS
                .Select(j => j.LLP)
                .FuturePlot(LLPLxyPlot, "JetMatched")
                .Save(dir);

            jetsWithLLPS
                .Select(j => j.LLP)
                .FuturePlot(LLPEtaPlot, "JetMatched")
                .Save(dir);

            // And look at the EMF as a function of the jet decay length so we can see exactly where the Calorimeter is.
            jetsWithLLPS
                .FuturePlot(JetCalRVsLXYPlot, "JetsWithLLPs")
                .Save(dir);

            // Check out what things look like for our cut region.
            LLPsToPlot
                .Where(llp => Constants.InCalorimeter.Invoke(llp.Lxy/1000))
                .FuturePlot(LLPLxyPlot, "In Cut CAL Range")
                .Save(dir);
        }

        /// <summary>
        /// Generate a series of plots
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="background"></param>
        /// <param name="plotter"></param>
        /// <param name="dir"></param>
        /// <param name="nameStub"></param>
        private static Tuple<IFutureValue<NTH1>, IFutureValue<NTH1>> CalcSignalToBackgroundSeries(IQueryable<JetInfoExtra> signal, IQueryable<JetInfoExtra> background,
            IPlotSpec<JetInfoExtra> plotter,
            FutureTDirectory dir, string nameStub)
        {
            // Do them all.
            CalcSignalToBackground(signal, background, plotter, dir, nameStub);
            // Do LLP that have LLPs
            // TODO: understand how the LLP association is made, and make sure it is good enough.
            CalcSignalToBackground(signal.Where(sj => sj.Jet.LLP.IsGoodIndex()), background, plotter, dir, $"{nameStub}LLPJ");
            // Has an LLP in the calorimeter.
            // TODO: understand how the LLP association is made, and make sure it is good enough.
            // TODO: Understand what isCRJet is defined to be.
            return CalcSignalToBackground(signal.Where(sj => sj.Jet.LLP.IsGoodIndex() && Constants.InCalorimeter.Invoke(sj.Jet.LLP.Lxy/1000)), background, plotter, dir, $"{nameStub}LLPJCal");
        }

        /// <summary>
        /// Calculate a set of plots over this list of events
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="background"></param>
        /// <param name="jetSelectorFunc"></param>
        /// <param name="dir"></param>
        private static Tuple<IFutureValue<NTH1>, IFutureValue<NTH1>> CalcSignalToBackground(IQueryable<JetInfoExtra> signal, IQueryable<JetInfoExtra> background,
            IPlotSpec<JetInfoExtra> plotter,
            FutureTDirectory dir,
            string name)
        {
            // Some generic plots
            signal
                .FuturePlot(JetExtraPtPlot, $"{name}_sig")
                .Save(dir);
            background
                .FuturePlot(JetExtraPtPlot, $"{name}_back")
                .Save(dir);

            signal
                .FuturePlot(JetExtraEtaPlot, $"{name}_sig")
                .Save(dir);
            background
                .FuturePlot(JetExtraEtaPlot, $"{name}_back")
                .Save(dir);

            // get the histograms for the signal and background
            var sPlot = signal
                .FuturePlot(plotter, "eff_sig")
                .Rename($"{name}_sig")
                .Save(dir)
                .Normalize()
                .AsCumulative(startWithZeroEff: false)
                .Rename($"{name}_sigrtback_sig")
                .Save(dir);
            var bPlot = background
                .FuturePlot(plotter, "eff_back")
                .Rename($"{name}_back")
                .Save(dir)
                .Normalize()
                .AsCumulative()
                .Rename($"{name}_sigrtback_back")
                .Save(dir);

            var bPlotSqrt = bPlot
                .Sqrt();

            // As a bonus, calc the effeciency graf. Get the x and y for that.
            // TODO: there doesn't seem to be a get accessor for Content on NTH1 - is that really right?
            //var r = from s in sPlot
            //        let sContent = Enumerable.Range(1, s.NbinsX).Select(idx => s.GetBinContent(idx)).ToArray()
            //        from b in bPlot
            //        select new ROOTNET.NTGraph(s.NBinsX, sContent, sContent);
            var effCurve = from s in sPlot
                           from b in bPlot
                           select CalculateROC(s, b, $"{name}_roc",$"ROC for {name}");
            effCurve
                .Save(dir);

            // Calc the S/sqrt(B) and return it.
            sPlot
                .DividedBy(bPlotSqrt)
                .Rename($"{name}_sigrtback")
                .Save(dir);

            return Tuple.Create(sPlot, bPlot);
        }

        /// <summary>
        /// Calculate on the fly the signal and rejection curves for the two plots.
        /// </summary>
        /// <param name="xAxisHist"></param>
        /// <param name="yAxisHist"></param>
        /// <returns></returns>
        private static NTGraph CalculateROC(NTH1 xAxisHist, NTH1 yAxisHist,
            string name, string title)
        {
            var tg = new ROOTNET.NTGraph(xAxisHist.NbinsX, xAxisHist.Data(), yAxisHist.Data());
            tg.Xaxis.Title = "Fractional Signal Efficiency";
            tg.Yaxis.Title = "Fractional Background Rejection";

            tg.Title = title;
            tg.Name = name;

            return tg;
        }
    }
}
