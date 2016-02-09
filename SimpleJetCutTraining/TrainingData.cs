using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleJetCutTraining
{
    /// <summary>
    /// Training data for simple jets
    /// </summary>
    class TrainingData
    {
        /// <summary>
        /// The locR for the jet
        /// </summary>
        public double logR;

        /// <summary>
        /// The number of tracks.
        /// </summary>
        public int nTracks;

        /// <summary>
        /// The pt of the lowest pT track
        /// that is near this jet axis.
        /// </summary>
        public double lowestPtTrack;
    }
}
