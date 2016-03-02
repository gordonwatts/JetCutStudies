using libDataAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var signal = (Files.Get600pi150lt9m())
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
                    .AsTTree(new FileInfo("backgroundTraining.root"));

                var signalTrainingData = signal
                    .AsTrainingTree()
                    .PlotTrainingVariables(outputHistograms.mkdir("signal"), "training_signal")
                    .AsTTree(new FileInfo("signalTraining.root"));

                // Now, do the training.

                // And, finally, the testing.
            }

        }
    }
}
