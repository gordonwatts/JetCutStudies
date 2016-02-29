using libDataAccess;
using System;
using System.Collections.Generic;
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
            // Our data sources
            var background = Files.GetAllJetSamples()
                .AsGoodJetStream();

            var signal = Files.Get600pi150lt9m().GenerateStream(1.0)
                .AsGoodJetStream();

            // We want to be flat w.r.t. pT, so reweight.
            var backgroundRewighted = background
                .PtSpectra()
                .WeightToMakeFlat(background, j => j.Jet.pT);

            // Finally, write out a tree for training everything.

            // Now, do the training.

            // And, finally, the testing.
        }
    }
}
