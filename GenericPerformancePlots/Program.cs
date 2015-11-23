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
                nFiles: 1, statusUpdate: l => Console.WriteLine(" -> " + l), intelligentLocal: true);

            var signalFiles = FindJobFiles("DiVertAnalysis", 3, "user.hrussell.mc15_13TeV.301303.HSS_mH125mS15.reco.s2698_r7144_EXT2",
                nFiles: 1, statusUpdate: l => Console.WriteLine(" -> " + l), intelligentLocal: true);

            var background = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFiles);
            var signal = DiVertAnalysis.QueryablerecoTree.CreateQueriable(signalFiles);

            // Output file
            // TODO: Creating a FutureHelper when no file is open causes a funny error message.
            Console.WriteLine("Opening outupt file");
            using (var outputHistograms = new FutureTFile("GenericPerformancePlots.root"))
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
                foreach (var ptRegion in Constants.PtRegions)
                {
                    CalcSignalToBackgroundSeries(
                        signal.SelectMany(events => events.Jets).Where(j => j.pT>= ptRegion.Item1 && j.pT < ptRegion.Item2),
                        background.SelectMany(events => events.Jets).Where(j => j.pT >= ptRegion.Item1 && j.pT < ptRegion.Item2),
                        JetCalRPlot,
                        outputHistograms.mkdir($"sigrtback_{ptRegion.Item1}_{ptRegion.Item2}"),
                        "CalR");
                }

                // Do a simple calc for info reasons which we will type out.
                var status = from nB in background.FutureCount()
                             from nS in signal.FutureCount()
                             select string.Format("Signal events: {0} Background events: {1}", nB, nS);

                // Run everything
                outputHistograms.Write();

                // Let the world know what is up
                Console.WriteLine(status.Value);
            }
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
        private static void CalcSignalToBackgroundSeries(IQueryable<recoTreeJets> signal, IQueryable<recoTreeJets> background,
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
            CalcSignalToBackground(signal.Where(sj => sj.LLP.IsGoodIndex() && Constants.InCalorimeter.Invoke(sj.LLP.Lxy/1000)), background, plotter, dir, $"{nameStub}LLPJCal");
        }

        /// <summary>
        /// Calculate a set of plots over this list of events
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="background"></param>
        /// <param name="jetSelectorFunc"></param>
        /// <param name="dir"></param>
        private static IFutureValue<NTH1> CalcSignalToBackground(IQueryable<recoTreeJets> signal, IQueryable<recoTreeJets> background,
            IPlotSpec<recoTreeJets> plotter,
            FutureTDirectory dir,
            string name)
        {
            // get the histograms for the signal and background
            var sPlot = signal
                .FuturePlot(plotter, "eff_back")
                .Normalize()
                .AsCumulative()
                .Rename($"{name}_sigrtback_sig")
                .Save(dir);
            var bPlot = background
                .FuturePlot(plotter, "eff_back")
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
            return sPlot
                .DividedBy(bPlotSqrt)
                .Rename($"{name}_sigrtback")
                .Save(dir);
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
            tg.Yaxis.Title = "Fractional Background Efficiency";

            tg.Title = title;
            tg.Name = name;

            return tg;
        }
    }
}
