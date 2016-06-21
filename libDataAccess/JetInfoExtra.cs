using DiVertAnalysis;
using libDataAccess.Utils;
using LINQToTreeHelpers;
using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Math;

namespace libDataAccess
{
    public class JetInfoExtra
    {
        public recoTreeJets Jet;
        public IEnumerable<recoTreeTracks> Tracks;
        public IEnumerable<recoTreeTracks> AllTracks;
    }

    public static class JetInfoExtraHelpers
    {
        /// <summary>
        /// Expression to create a jet info extra object from an event and a jet.
        /// </summary>
        public static Expression<Func<recoTree, recoTreeJets, JetInfoExtra>> CreateJetInfoExtra = (ev, j) => new JetInfoExtra()
        {
            Jet = j,
            Tracks = ev.Tracks.Where(t => t.pT >= Constants.TrackJetAssociationMinPt && ROOTUtils.DeltaR2(j.eta, j.phi, t.eta, t.phi) < Constants.TrackJetAssociationDR2),
            AllTracks = ev.Tracks.Where(t => t.pT >= Constants.TrackJetAssociationAllMinPt && ROOTUtils.DeltaR2(j.eta, j.phi, t.eta, t.phi) < Constants.TrackJetAssociationDR2),
        };

        /// <summary>
        /// Build the jet info extra class from a regular event.
        /// </summary>
        /// <param name="background"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: Track quality cuts, not just track and pt
        /// </remarks>
        public static IQueryable<JetInfoExtra> BuildSuperJetInfo(IQueryable<recoTree> background)
        {
            return background
                .SelectMany(ev => ev.Jets.Where(j => IsGoodJet.Invoke(j)).Select(j => CreateJetInfoExtra.Invoke(ev, j)));
        }

        /// <summary>
        /// Test for good jets
        /// </summary>
        public static Expression<Func<recoTreeJets, bool>> IsGoodJet = j => j.pT > 40.0 && Abs(j.eta) < 2.5;
    }

}
