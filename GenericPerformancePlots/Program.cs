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

using static libDataAccess.PlotSpecifications;
using static GenericPerformancePlots.GRIDJobs;

namespace GenericPerformancePlots
{
    class Program
    {
        /// <summary>
        /// Make generic plots of the signal or background
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var backgroundFiles = FindJobFiles("DiVertAnalysis", 3, "user.emmat.mc15_13TeV.361022.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ2W.merge.AOD.e3668_s2576_s2132_r6765_r6282__EXOT15_v3_EXT0",
                nFiles: 1, statusUpdate: l => Console.WriteLine(" -> " + l), intelligentLocal: true);

            var signalFiles = FindJobFiles("DiVertAnalysis", 3, "user.hrussell.mc15_13TeV.301303.HSS_mH125mS15.reco.s2698_r7144_EXT2",
                nFiles: 1, statusUpdate: l => Console.WriteLine(" -> " + l), intelligentLocal: true);

            var background = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFiles);
            var signal = DiVertAnalysis.QueryablerecoTree.CreateQueriable(signalFiles);

            // Output file
            // TODO: Creating a FutureHelper when no file is open causes a funny error message.
            var outputHistograms = new FutureTFile("GenericPerformancePlots.root");

            background
                .SelectMany(events => events.Jets)
                .PlotBasicDataPlots(outputHistograms.mkdir("background"), "all");
            signal
                .SelectMany(events => events.Jets)
                .PlotBasicDataPlots(outputHistograms.mkdir("signal"), "all");

            // Cal efficiency plots for CalR
            CalcSignalToBackground(
                signal.SelectMany(events => events.Jets),
                background.SelectMany(events => events.Jets),
                JetCalRPlot,
                outputHistograms.mkdir("sigrtback"),
                "CalR");

            // Do a simple calc for info reasons which we will type out.
            var status = from nB in background.FutureCount()
                         from nS in signal.FutureCount()
                         select string.Format("Signal events: {0} Background events: {1}", nB, nS);

            // Run everything
            outputHistograms.Write();

            // Let the world know what is up
            Console.WriteLine(status.Value);
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
                .Save(dir)
                .Sqrt();

            return sPlot
                .DividedBy(bPlot)
                .Rename($"{name}_sigrtback")
                .Save(dir);
        }
    }
}
