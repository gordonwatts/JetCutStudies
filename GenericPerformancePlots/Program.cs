using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LINQToTreeHelpers;
using System.Text;
using System.Threading.Tasks;
using LINQToTTreeLib;
using LINQToTreeHelpers.FutureUtils;
using DiVertAnalysis;
using static LINQToTreeHelpers.PlottingUtils;
using LinqToTTreeInterfacesLib;
using ROOTNET.Interface;
using libDataAccess;

using static libDataAccess.PlotSpecifications;
using static GenericPerformancePlots.GRIDJobs;
using static System.Linq.Enumerable;
using libDataAccess.Utils;

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
        /// </remarks>
        static void Main(string[] args)
        {
            Console.WriteLine("Finding the files");
            var backgroundFiles = FindJobFiles("DiVertAnalysis", 3, "user.emmat.mc15_13TeV.361022.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ2W.merge.AOD.e3668_s2576_s2132_r6765_r6282__EXOT15_v3_EXT0",
                nFiles: 0, statusUpdate: l => Console.WriteLine(" -> " + l), intelligentLocal: true);

            var signalHV125pi15 = FindJobFiles("DiVertAnalysis", 3, "user.hrussell.mc15_13TeV.301303.HSS_mH125mS15.reco.s2698_r7144_EXT2",
                nFiles: 0, statusUpdate: l => Console.WriteLine(" -> " + l), intelligentLocal: true);

            var signalHV125pi40 = FindJobFiles("DiVertAnalysis", 3, "user.hrussell.mc15_13TeV.301298.HSS_mH125mS40.reco_20k.s2698_r7144_v03_EXT2",
                nFiles: 0, statusUpdate: l => Console.WriteLine(" -> " + l), intelligentLocal: true);

            var signalHV600pi100 = FindJobFiles("DiVertAnalysis", 3, "user.hrussell.mc15_13TeV.301301.HSS_mH600mS100.reco_20k.s2698_r7144_v03_EXT2",
                nFiles: 0, statusUpdate: l => Console.WriteLine(" -> " + l), intelligentLocal: true);

            var backgroundEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFiles);
            //backgroundEvents.IgnoreQueryCache = true;
            backgroundEvents.UseStatementOptimizer = false;
            var signalHV125pi15Events = DiVertAnalysis.QueryablerecoTree.CreateQueriable(signalHV125pi15);
            //signalHV125pi15Events.IgnoreQueryCache = true;
            signalHV125pi15Events.UseStatementOptimizer = false;
            var signalHV125pi40Events = DiVertAnalysis.QueryablerecoTree.CreateQueriable(signalHV125pi40);
            //signalHV125pi40Events.IgnoreQueryCache = true;
            signalHV125pi40Events.UseStatementOptimizer = false;
            var signalHV600pi100Events = DiVertAnalysis.QueryablerecoTree.CreateQueriable(signalHV600pi100);
            //signalHV600pi100Events.IgnoreQueryCache = true;
            signalHV600pi100Events.UseStatementOptimizer = false;

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
            background
                .SelectMany(events => events.Jets)
                .PlotBasicDataPlots(outputHistograms.mkdir("background"), "all");
            signal
                .SelectMany(events => events.Jets)
                .PlotBasicDataPlots(outputHistograms.mkdir("signal"), "all");

            // Some basic info about the LLP's
            LLPBasicInfo(signal.SelectMany(s => s.LLPs), signal.SelectMany(s => s.Jets), outputHistograms.mkdir("signalLLP"));

            // Cal efficiency plots for CalR
            CalcSignalToBackgroundSeries(
                signal.SelectMany(events => events.Jets),
                background.SelectMany(events => events.Jets),
                JetCalRPlot,
                outputHistograms.mkdir("sigrtback"),
                "CalR");

            // Next, as a function of pT
            var backValues = new double[] { 0.001, 0.01, 0.05, 0.1 };
            var sigValues = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 };
            var result = new List<IFutureValue<string>>();
            foreach (var ptRegion in Constants.PtRegions)
            {
                var dir = outputHistograms.mkdir($"sigrtback_{ptRegion.Item1}_{ptRegion.Item2}");

                var signalGoodJets = signal
                    .SelectMany(events => events.Jets)
                    .Where(j => j.pT >= ptRegion.Item1 && j.pT < ptRegion.Item2);

                var backgroundGoodJets = background
                    .SelectMany(events => events.Jets)
                    .Where(j => j.pT >= ptRegion.Item1 && j.pT < ptRegion.Item2);

                var sigBack = CalcSignalToBackgroundSeries(
                    signalGoodJets,
                    backgroundGoodJets,
                    JetCalRPlot,
                    dir,
                    "CalR");

                var requiredBackValues = backValues
                    .Select(bv => from r in sigBack.Item2 select $"{ptRegion.Item1}: Cut for Back Eff of {bv} is {CalcEffValue(r, bv)}");
                result.AddRange(requiredBackValues);
                var requiredSigValues = sigValues
                    .Select(sv => from r in sigBack.Item1 select $"{ptRegion.Item1}: Cut for Sig eff of {sv} is {CalcEffValue(r, sv, false)}");
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
        private static Tuple<IFutureValue<NTH1>, IFutureValue<NTH1>> CalcSignalToBackgroundSeries(IQueryable<recoTreeJets> signal, IQueryable<recoTreeJets> background,
            IPlotSpec<recoTreeJets> plotter,
            FutureTDirectory dir, string nameStub)
        {
            // Do them all.
            CalcSignalToBackground(signal, background, plotter, dir, nameStub);
            // Do LLP that have LLPs
            // TODO: understand how the LLP association is made, and make sure it is good enough.
            CalcSignalToBackground(signal.Where(sj => sj.LLP.IsGoodIndex()), background, plotter, dir, $"{nameStub}LLPJ");
            // Has an LLP in the calorimeter.
            // TODO: understand how the LLP association is made, and make sure it is good enough.
            // TODO: Understand what isCRJet is defined to be.
            return CalcSignalToBackground(signal.Where(sj => sj.LLP.IsGoodIndex() && Constants.InCalorimeter.Invoke(sj.LLP.Lxy/1000)), background, plotter, dir, $"{nameStub}LLPJCal");
        }

        /// <summary>
        /// Calculate a set of plots over this list of events
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="background"></param>
        /// <param name="jetSelectorFunc"></param>
        /// <param name="dir"></param>
        private static Tuple<IFutureValue<NTH1>, IFutureValue<NTH1>> CalcSignalToBackground(IQueryable<recoTreeJets> signal, IQueryable<recoTreeJets> background,
            IPlotSpec<recoTreeJets> plotter,
            FutureTDirectory dir,
            string name)
        {
            // Some generic plots
            signal
                .FuturePlot(JetPtPlot, $"{name}_sig")
                .Save(dir);
            background
                .FuturePlot(JetPtPlot, $"{name}_back")
                .Save(dir);

            signal
                .FuturePlot(JetEtaPlot, $"{name}_sig")
                .Save(dir);
            background
                .FuturePlot(JetEtaPlot, $"{name}_back")
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
