using DiVertAnalysis;
using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static libDataAccess.Files;
using static System.Math;
using static libDataAccess.CutConstants;
using libDataAccess;
using static libDataAccess.JetInfoExtraHelpers;
using System.Linq.Expressions;

namespace libDataAccess.Utils
{
    /// <summary>
    /// Jet info we need for training
    /// </summary>
    public class JetStream
    {
        public JetInfoExtra JetInfo;
        public double Weight;
        public int EventNumber;
        public int RunNumber;
    }

    public static class SampleUtils
    {
        /// <summary>
        /// Turn an event sample into a jet sample.
        /// Apply default cuts as well.
        /// </summary>
        public static IQueryable<JetStream> AsGoodJetStream(this IQueryable<MetaData> source)
        {
            return source
                .SelectMany(e => e.Data.Jets.Select(j => new JetStream() { JetInfo = CreateJetInfoExtra.Invoke(e.Data, j), Weight = e.xSectionWeight, EventNumber = e.Data.eventNumber, RunNumber = e.Data.runNumber }))
                .Where(j => j.JetInfo.Jet.pT > 40.0 && Abs(j.JetInfo.Jet.eta) < JetEtaLimit)
                ;
        }

        /// <summary>
        /// Get a good jet stream that is the high pT jet.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<JetStream> AsGoodFirstJetStream(this IQueryable<MetaData> source)
        {
            return source
                .Select(e => new JetStream() { JetInfo = CreateJetInfoExtra.Invoke(e.Data, e.Data.Jets.OrderByDescending(j => j.pT).First()), Weight = e.xSectionWeight, EventNumber = e.Data.eventNumber, RunNumber = e.Data.runNumber })
                .Where(j => j.JetInfo.Jet.pT > 40.0 && Abs(j.JetInfo.Jet.eta) < JetEtaLimit)
                ;

        }

        /// <summary>
        /// Cut to determine if this is a good signal jet.
        /// </summary>
        //public static Expression<Func<recoTreeJets, bool>> IsGoodSignalJet = j =>
        //        (j.LLP.IsGoodIndex() && j.LLP.Lxy > InnerDistanceForSignalLLPBarrelDecay);
        public static Expression<Func<recoTreeJets, bool>> IsGoodSignalJet = j =>
           Math.Abs(j.eta) <= 1.7
                ? (j.LLP.IsGoodIndex() && j.LLP.Lxy > InnerDistanceForSignalLLPBarrelDecay)
                : (j.LLP.IsGoodIndex() && j.LLP.Lz > InnerDistanceForSignalLLPEndcapDecay);

        /// <summary>
        /// Make sure we are talking about good signal only.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<JetStream> FilterSignal(this IQueryable<JetStream> source)
        {
            return source
                .Where(j => IsGoodSignalJet.Invoke(j.JetInfo.Jet));
        }

        /// <summary>
        /// Only jets with a LLP near by.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<JetStream> FilterLLPNear(this IQueryable<JetStream> source)
        {
            return source
                .Where(j => j.JetInfo.Jet.LLP.IsGoodIndex());
        }

        /// <summary>
        /// Return only training events
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<JetStream> FilterTrainingEvents(this IQueryable<JetStream> source)
        {
            return source
                .Where(j => !(j.EventNumber % 3 == 1));
        }

        /// <summary>
        /// Filter for non-training events
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<JetStream> FilterNonTrainingEvents(this IQueryable<JetStream> source)
        {
            return source
                .Where(j => !(j.EventNumber % 3 == 1));
        }
    }
}
