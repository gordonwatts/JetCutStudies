using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib.Files;
using System;
using LINQToTTreeLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static JetMVATraining.SampleUtils;
using LinqToTTreeInterfacesLib;

namespace JetMVATraining
{
    class Program
    {
        /// <summary>
        /// Run the training for the MVA. This is run in a library,
        /// so basically behind-this-guys-back. But it provides an easy single item to run.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Parse command line arguments
            CommandLineUtils.Parse(args);

            // Our data sources
            var background = Files.GetAllJetSamples()
                .AsGoodJetStream();

            var signal = (Files.Get600pi150lt9m().Concat(Files.Get200pi25lt5m()).Concat(Files.Get400pi100lt9m()))
                .GenerateStream(1.0)
                .AsGoodJetStream()
                .FilterSignal();

            using (var outputHistograms = new FutureTFile("JetMVATraining.root"))
            {
                // Plot the pt spectra before flattening.
                background = background
                    .FlattenPtSpectra(outputHistograms, "background");
                signal = signal
                    .FlattenPtSpectra(outputHistograms, "signal");

                // Finally, write out a tree for training everything.
                var backgroundTrainingData = background
                    .AsTrainingTree()
                    .PlotTrainingVariables(outputHistograms.mkdir("background"), "training_background")
                    .AsTTree(treeName: "TrainingTree", outputROOTFile: new FileInfo("backgroundTraining.root"));

                var signalTrainingData = signal
                    .AsTrainingTree()
                    .PlotTrainingVariables(outputHistograms.mkdir("signal"), "training_signal")
                    .AsTTree(treeName: "TrainingTree", outputROOTFile: new FileInfo("signalTraining.root"));

                // Now, do the training.

                // And, finally, generate some efficiency plots.

                var cuts = new CutInfo[]
                {
                    new CutInfo() {Title="Run1", Cut = js => js.JetInfo.Jet.logRatio > 1.2 && !js.JetInfo.Tracks.Any() },
                };

                // Calc the background efficiency for the standard Run 1 cut.
                var standardBackgroundEff = background
                    .CalcualteEfficiency(cuts[0].Cut, js => js.Weight);



                foreach (var c in cuts)
                {
                    //GenerateEfficiencyPlots(outputHistograms.mkdir(c.Title), c.Cut);
                }
            }

        }

        /// <summary>
        /// Generate the required efficiency plots
        /// </summary>
        /// <param name="futureTDirectory"></param>
        /// <param name="cut"></param>
        private static IFutureValue<double> GenerateEfficiencyPlots(FutureTDirectory futureTDirectory, Expression<Func<JetStream, bool>> cut, IQueryable<JetStream> source)
        {
            // Calculate the overall efficiency of this guy.
            var eff = source.CalcualteEfficiency(cut, js => js.Weight);

            return eff;
        }

        private class CutInfo
        {
            /// <summary>
            /// The actual cut
            /// </summary>
            public Expression<Func<JetStream, bool>> Cut;

            /// <summary>
            /// What we should be calling this thing
            /// </summary>
            public string Title;


        }
    }
}
