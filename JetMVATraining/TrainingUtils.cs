using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JetMVATraining.SampleUtils;

namespace JetMVATraining
{
    /// <summary>
    /// The info we will hand to the training
    /// </summary>
    public class TrainingTree
    {
        public double Weight;
        public double JetPt;
        public double CalRatio;
        public double JetEta;
    }

    /// <summary>
    /// Utility functions to setup the training.
    /// </summary>
    public static class TrainingUtils
    {
        internal static IQueryable<TrainingTree> AsTrainingTree(this IQueryable<JetStream> source)
        {
            return source
                .Select(i => new TrainingTree()
                {
                    Weight = i.Weight,
                    CalRatio = i.Jet.logRatio,
                    JetPt = i.Jet.pT,
                    JetEta = i.Jet.eta,
                });
        }
    }
}
