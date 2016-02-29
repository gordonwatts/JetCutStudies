using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static libDataAccess.Files;

namespace JetMVATraining
{
    static class SampleUtils
    {
        /// <summary>
        /// Jet info we need for training
        /// </summary>
        public class JetStream
        {
            public recoJet Jet;
            public double Weight;
        }

        /// <summary>
        /// Turn an event sample into a jet sample.
        /// Apply default cuts as well.
        /// </summary>
        public static IQueryable<JetStream> AsGoodJetStream(this IQueryable<MetaData> source)
        {
            return source
                .SelectMany(e => e.Data.Jets.Select(j => new JetStream() { Jet = j, Weight = e.xSectionWeight }))
                .Where(j => j.Jet.pT > 40.0 && Abs(j.Jet.Eta) < 2.4);
        }
    }
}
