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
            var backgroundFile = new FileInfo(@"C:\Users\gordo\Documents\GRIDDS\user.gwatts.361022.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ2W.r6765_r6282__EXOT15_v3_EXT0.DiVertAnalysis_v1_242636232_hist\user.gwatts\user.gwatts.6923254._000001.hist-output.root");
            if (!backgroundFile.Exists)
            {
                throw new ArgumentException("Can't open file");
            }
            var signalFile = new FileInfo(@"C:\Users\gordo\Documents\GRIDDS\user.gwatts.301303.HSS_mH125mS15.reco.s2698_r7144_EXT2.DiVertAnalysis_v2_264553385_hist\user.gwatts\user.gwatts.6988346._000001.hist-output.root");
            if (!signalFile.Exists)
            {
                throw new ArgumentException("Can't open signal file");
            }

            var background = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFile);
            var signal = DiVertAnalysis.QueryablerecoTree.CreateQueriable(signalFile);

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
