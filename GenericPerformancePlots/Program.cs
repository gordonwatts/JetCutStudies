﻿using CommandLine;
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
using System.Threading.Tasks;
using static libDataAccess.JetInfoExtraHelpers;
using static libDataAccess.PlotSpecifications;
using static libDataAccess.Utils.CommandLineUtils;
using static libDataAccess.Utils.Constants;
using static LINQToTreeHelpers.PlottingUtils;
using static System.Linq.Enumerable;

namespace GenericPerformancePlots
{
    class Program
    {
        class Options : CommonOptions
        {
            [Option("SmallTestingMenu", HelpText = "If present, then run on a small number of samples", Default = false)]
            public bool SmallTestingMenu { get; set; }
        }

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
        static async Task Main(string[] args)
        {
            ConsoleMessageDumper.SetupConsoleMessageDumper();

            var opt = CommandLineUtils.ParseOptions<Options>(args);
            var preferedPlacesToRun = new[] { "UWTeV-linux" };

            Console.WriteLine("Setting up the queries");

            // All the background samples have to be done first.
            var backgroundSamples = Task.WhenAll(
                    SampleMetaData.AllSamplesWithTag("mc15c", "background")
                    .Select(async info => Tuple.Create(await Files.GetSampleAsMetaData(info, preferPlaces: preferedPlacesToRun), info.NickName))
                    .ToArray()
                    );

            var backgroundEvents = Files.GetAllJetSamples(preferedPlacesToRun)
                .ContinueWith(r => r.Result.Select(e => e.Data));

            // All the signal we are going to make plots of.
            var tags = new[] { "mc15c", "signal", "hss" };
            if (opt.SmallTestingMenu)
            {
                tags = tags.Add("quick_compare").ToArray();
            }
            var signalSamples = Task.WhenAll(
                    SampleMetaData.AllSamplesWithTag(tags)
                        .Select(async info => Tuple.Create(await Files.GetSampleAsMetaData(info, preferPlaces: preferedPlacesToRun), info.NickName))
                        .ToArray()
                );

            // Get the beam-halo samples to use for testing and training
            var data15 = SampleMetaData.AllSamplesWithTag("data15_p2950")
                .Take(opt.UseFullDataset ? 10000 : 2)
                .SamplesAsSingleQueriable(preferPlaces: preferedPlacesToRun)
                .ContinueWith(r => r.Result.AsBeamHaloStream(SampleUtils.DataEpoc.data15));

            var data16 = SampleMetaData.AllSamplesWithTag("data16_p2950")
                .Take(opt.UseFullDataset ? 10000 : 1)
                .SamplesAsSingleQueriable(preferPlaces: preferedPlacesToRun)
                .ContinueWith(r => r.Result.AsBeamHaloStream(SampleUtils.DataEpoc.data16));

            // Parallelize looking and running everything. This makes the below code execute more smoothly (I think).
            await Task.WhenAll(backgroundSamples, backgroundEvents, signalSamples, data15, data16);

            // Output file
            Console.WriteLine("Opening output file & Running the queries");
            using (var outputHistograms = new FutureTFile("GenericPerformancePlots.root"))
            {
                // First, lets do a small individual thing for each individual background sample.
                var bkgDir = outputHistograms.mkdir("background");

                Console.WriteLine("Making background plots.");
                foreach (var background in backgroundSamples.Result)
                {
                    Console.WriteLine(background.Item2);
                    NoGCExecute(() =>
                    {
                        var d = bkgDir.mkdir(background.Item2);
                        background.Item1.Select(m => (double) m.Data.actualIntPerCrossing)
                            .FuturePlot(EventInteractionsPerCossing, "all")
                            .Save(d);
                        BuildSuperJetInfo(background.Item1.Select(md => md.Data))
                        .PlotBasicDataPlots(d, "all");
                    });
                }

                Console.WriteLine("data15");
                NoGCExecute(() =>
                {
                    var dr = bkgDir.mkdir("data15");
                    data15.Result.Select(m => (double) m.Data.actualIntPerCrossing)
                        .FuturePlot(EventInteractionsPerCossing, "all")
                        .Save(dr);
                    BuildSuperJetInfo(data15.Result.Select(d => d.Data))
                        .PlotBasicDataPlots(dr, "all");
                });

                Console.WriteLine("data16");
                NoGCExecute(() =>
                {
                    var dr = bkgDir.mkdir("data16");
                    data16.Result.Select(m => (double) m.Data.actualIntPerCrossing)
                        .FuturePlot(EventInteractionsPerCossing, "all")
                        .Save(dr);
                    BuildSuperJetInfo(data16.Result.Select(d => d.Data))
                        .PlotBasicDataPlots(dr, "all");
                });

                // Do a quick study for each signal sample, using all the backgrounds at once to make
                // performance plots. 
                Console.WriteLine("Making the signal/background plots.");
                foreach (var sample in signalSamples.Result)
                {
                    Console.WriteLine(sample.Item2);
                    var w = outputHistograms.mkdir(sample.Item2);
                    NoGCExecute(() =>
                    {
                        var status = PerSampleStudies(backgroundEvents.Result, sample.Item1.Select(md => md.Data), w);
                        DumpResults($"Sample {sample.Item2}:", status);
                    });
                }
                // Write out the histograms
                outputHistograms.Write();
                outputHistograms.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
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
        /// Run an action with the GC suppressed (as much as it can)
        /// </summary>
        /// <remarks>
        /// This is done because there is a bug in an interaction between ROOT and the GC. Finalizers are were
        /// most objects are deleted. But if on the main thread something is being created at the same time, you
        /// can run into a race condition. Turnning off the GC temporarily suppresses the running of the Finalizers
        /// for a short while - as long as memory allocation doesn't get out of hand. When this bug is actually fixed
        /// we might be ok to remove this.
        /// 
        /// Might need to wait until ROOT v6 before that is the case.
        /// </remarks>
        /// <param name="noGCBuffer"></param>
        /// <param name="a"></param>
        private static void NoGCExecute (Action a)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var oldMode = System.Runtime.GCSettings.LatencyMode;
            try
            {
                System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;
                a();
            } finally
            {
                System.Runtime.GCSettings.LatencyMode = oldMode;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
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

            NoGCExecute(() =>
            {
                backgroundJets
                    .PlotBasicDataPlots(outputHistograms.mkdir("background"), "all");
            });

            var sigdir = outputHistograms.mkdir("signal");
            NoGCExecute(() =>
            {
                signalJets
                    .PlotBasicDataPlots(sigdir, "all");
                signalJets
                    .PlotBasicSignalPlots(sigdir, "all");
            });

            var result = new List<IFutureValue<string>>();
            NoGCExecute(() =>
            {
                signalJets
                    .Where(j => j.Jet.LLP.IsGoodIndex())
                    .PlotBasicDataPlots(sigdir, "withLLP");
                signalJets
                    .Where(j => j.Jet.LLP.IsGoodIndex())
                    .PlotBasicSignalPlots(sigdir, "withLLP");
            });

            NoGCExecute(() =>
            {
                signalJets
                    .Where(j => j.Jet.LLP.IsGoodIndex())
                    .Where(j => LLPInCalorimeter.Invoke(j.Jet.LLP))
                    .PlotBasicDataPlots(sigdir, "withLLPInCal");
                signalJets
                    .Where(j => j.Jet.LLP.IsGoodIndex())
                    .Where(j => LLPInCalorimeter.Invoke(j.Jet.LLP))
                    .PlotBasicSignalPlots(sigdir, "withLLPInCal");
            });
#if false

            // Some basic info about the LLP's
            // TODO: make sure this is part of the LLPInvestigations guy.
            // LLPBasicInfo(signal.SelectMany(s => s.LLPs), signal.SelectMany(s => s.Jets), outputHistograms.mkdir("signalLLP"));

            // Do the CalR and NTrk plots
            var rtot = CalSigAndBackgroundSeries(signalJets, backgroundJets, "all", outputHistograms.mkdir("sigrtback"));
            result.AddRange(result);
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Next, as a function of pT
            foreach (var ptRegion in Constants.PtRegions)
            {
                var dir = outputHistograms.mkdir($"sigrtback_{ptRegion.Item1}_{ptRegion.Item2}");

                var signalGoodJets = signalJets
                    .Where(j => j.Jet.pT >= ptRegion.Item1 && j.Jet.pT < ptRegion.Item2);

                var backgroundGoodJets = backgroundJets
                    .Where(j => j.Jet.pT >= ptRegion.Item1 && j.Jet.pT < ptRegion.Item2);

                var regionInfo = $"{ptRegion.Item1}-{ptRegion.Item2}";

                var r = CalSigAndBackgroundSeries(signalGoodJets, backgroundGoodJets, regionInfo, dir);

                result.AddRange(r);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Dump out the number of events so everyone can see.
            var status = from nB in background.FutureCount()
                         from nS in signal.FutureCount()
                         select string.Format("Signal events: {0} Background events: {1}", nS, nB);
            result.Add(status);
#endif
            return result;
        }

        /// <summary>
        /// For a set of signal and background nets, do the calc for ntrk, cal ratio, etc.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="signalGoodJets"></param>
        /// <param name="backgroundGoodJets"></param>
        /// <param name="regionInfo"></param>
        /// <returns></returns>
        private static List<IFutureValue<string>> CalSigAndBackgroundSeries(IQueryable<JetInfoExtra> signalGoodJets, IQueryable<JetInfoExtra> backgroundGoodJets, string regionInfo, FutureTDirectory dir)
        {
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

            var sigBackSumPt = CalcSignalToBackgroundSeries(
                signalGoodJets,
                backgroundGoodJets,
                SumTrackPtPlot,
                dir,
                "SumPt");

            var sigBackMaxPt = CalcSignalToBackgroundSeries(
                signalGoodJets,
                backgroundGoodJets,
                MaxTrackPtPlot,
                dir,
                "MaxPt");

            // Look to see what it would take to get constant efficiency
            var result = new List<IFutureValue<string>>();
#if false
            // This was interesting - but not yet sure how to use it.
            var backRejValues = new double[] { 0.999, 0.99, 0.95, 0.9 };
            var sigValues = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 };

            var requiredBackValues = from bv in backRejValues
                                     select from bHist in sigBackCalR.Item2
                                            from sHist in sigBackCalR.Item1
                                            let backCut = CalcEffValue(bHist, bv)
                                            let effValue = LookupEffAtCut(sHist, backCut)
                                            select $"{regionInfo}: Cut for Back Rejection of {bv} is {backCut} (sig eff is {effValue})";
            result.AddRange(requiredBackValues);

            var requiredSigValues = from sv in sigValues
                                    select from bHist in sigBackCalR.Item2
                                           from sHist in sigBackCalR.Item1
                                           let sigCut = CalcEffValue(sHist, sv, false)
                                           let backRejection = LookupEffAtCut(bHist, sigCut)
                                           select $"{regionInfo}: Cut for Sig Eff of {sv} is {sigCut} (back rej is {backRejection})";
            result.AddRange(requiredSigValues);
#endif

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
        /// Return the x-axis value for a particular efficiency.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="bv"></param>
        /// <returns></returns>
        private static double CalcEffValue(NTH1 r, double bv, bool greater = true)
        {
            var firstValue = Range(0, r.NbinsX)
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
            // LLP's and LLP's associated with a jet
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
            GC.Collect(3, GCCollectionMode.Forced, true);
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
