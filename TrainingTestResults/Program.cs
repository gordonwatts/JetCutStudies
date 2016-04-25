using JenkinsAccess;
using libDataAccess.Utils;
using LINQToTreeHelpers.FutureUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingTestResults
{
    class Program
    {
        class MVAInfo
        {
            /// <summary>
            ///  The uri to the artifact for this mva
            /// </summary>
            public Uri Artifact;

            /// <summary>
            /// Short name we can use in plots, etc., for the mva.
            /// </summary>
            public string Name;
        }

        /// <summary>
        /// Look at a number of MVA trainings and use them as results.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Parse the arguments.
            CommandLineUtils.Parse(args);

            // List the artifacts that we are going to be using.
            var mvaResults = new MVAInfo[]
            {
                new MVAInfo() { Name = "FirstPt", Artifact = new Uri("http://jenks-higgs.phys.washington.edu:8080/job/CalR-JetMVATraining/372/artifact/Jet.MVATraining-JetPt.CalRatio.NTracks.SumPtOfAllTracks.MaxTrackPt_BDT.weights.xml") },
                new MVAInfo() { Name = "FirstET", Artifact = new Uri("http://jenks-higgs.phys.washington.edu:8080/job/CalR-JetMVATraining/374/artifact/Jet.MVATraining-CalRatio.NTracks.SumPtOfAllTracks.MaxTrackPt.JetET_BDT.weights.xml") },
            };

            // Fill an output file with the info for each MVA
            using (var f = new FutureTFile("TrainingTestResults.root"))
            {
                foreach (var mva in mvaResults)
                {
                    // The directory
                    var d = f.mkdir(mva.Name);

                    // Get the weight file.
                    var weights = ArtifactAccess.GetArtifactFile(mva.Artifact);

                    // Now, create the mva value from the weight file
                }
            }
        }
    }
}
