using DiVertAnalysis;
using libDataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Build the jet info extra class from a regular event.
        /// </summary>
        /// <param name="background"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: Track quality cuts, not just track and pt
        /// </remarks>
        public static IQueryable<JetInfoExtra> BuildSuperJetInfo(IQueryable<recoTree> background)
        {
            return from ev in background
                   from j in ev.Jets
                   where j.pT > 40.0 && Abs(j.eta) < 2.5
                   select new JetInfoExtra()
                   {
                       Jet = j,
                       Tracks = ev.Tracks.Where(t => t.pT >= Constants.TrackJetAssociationMinPt && ROOTUtils.DeltaR2(j.eta, j.phi, t.eta, t.phi) < Constants.TrackJetAssociationDR2),
                       AllTracks = ev.Tracks.Where(t => t.pT >= Constants.TrackJetAssociationAllMinPt && ROOTUtils.DeltaR2(j.eta, j.phi, t.eta, t.phi) < Constants.TrackJetAssociationDR2),
                   };
        }
    }

}
