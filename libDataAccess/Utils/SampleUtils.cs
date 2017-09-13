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
        public double InteractionsPerCrossing;
    }

    public static class SampleUtils
    {
        /// <summary>
        /// Turn an event sample into a jet sample.
        /// Apply default cuts as well.
        /// </summary>
        public static IQueryable<JetStream> AsGoodJetStream(this IQueryable<MetaData> source, double pTCut = 40.0)
        {
            return source
                .SelectMany(e => e.Data.Jets.Select(j => new JetStream() { JetInfo = CreateJetInfoExtra.Invoke(e.Data, j), Weight = e.xSectionWeight, EventNumber = e.Data.eventNumber, RunNumber = e.Data.runNumber, InteractionsPerCrossing = e.Data.actualIntPerCrossing }))
                .Where(j => j.JetInfo.Jet.pT > pTCut && Abs(j.JetInfo.Jet.eta) < JetEtaLimit)
                ;
        }

        /// <summary>
        /// Which part of the data is this?
        /// </summary>
        public enum DataEpoc
        {
            data15,
            data16
        }

        /// <summary>
        /// Return events that are beam halo events (as far as we can tell).
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<MetaData> AsBeamHaloStream(this IQueryable<MetaData> source, DataEpoc epoc)
        {
            switch (epoc)
            {
                case DataEpoc.data15:
                    return source
                        .Where(s => s.Data.event_passCalRatio_TAU60_noiso && !s.Data.event_passCalRatio_TAU60);
                case DataEpoc.data16:
                    return source
                        .Where(s => s.Data.event_passCalRatio_cleanLLP_TAU60_noiso && !s.Data.event_passCalRatio_cleanLLP_TAU60);
                default:
                    throw new InvalidOperationException($"Unknown DataEpoc {epoc} - no idea how to do beam halo");
            }
        }

        /// <summary>
        /// Get a good jet stream that is the high pT jet.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<JetStream> AsGoodFirstJetStream(this IQueryable<MetaData> source, double pTCut = 40.0)
        {
            return source
                .Select(e => new JetStream() { JetInfo = CreateJetInfoExtra.Invoke(e.Data, e.Data.Jets.OrderByDescending(j => j.pT).First()), Weight = e.xSectionWeight, EventNumber = e.Data.eventNumber, RunNumber = e.Data.runNumber, InteractionsPerCrossing = e.Data.actualIntPerCrossing })
                .Where(j => j.JetInfo.Jet.pT > pTCut && Abs(j.JetInfo.Jet.eta) < JetEtaLimit)
                ;

        }

        /// <summary>
        /// Cut to determine if this is a good signal jet.
        /// </summary>
        public static Expression<Func<recoTreeJets, bool>> IsGoodSignalJet = j =>
           j.LLP.IsGoodIndex();
        //public static Expression<Func<recoTreeJets, bool>> IsGoodSignalJet = j =>
        //   Math.Abs(j.eta) <= 1.7
        //        ? (j.LLP.IsGoodIndex() && j.LLP.Lxy > InnerDistanceForSignalLLPBarrelDecay)
        //        : (j.LLP.IsGoodIndex() && j.LLP.Lz > InnerDistanceForSignalLLPEndcapDecay);

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
