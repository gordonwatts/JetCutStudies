using DiVertAnalysis;
using libDataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                   select new JetInfoExtra()
                   {
                       Jet = j,
                       Tracks = ev.Tracks.Where(t => t.pT >= Constants.TrackJetAssociationMinPt && ROOTUtils.DeltaR2(j.eta, j.phi, t.eta, t.phi) < Constants.TrackJetAssociationDR2),
                       AllTracks = ev.Tracks.Where(t => t.pT >= Constants.TrackJetAssociationAllMinPt && ROOTUtils.DeltaR2(j.eta, j.phi, t.eta, t.phi) < Constants.TrackJetAssociationDR2),
                   };
        }

#if false
        private static IQueryable<recoTreeTracks> TracksNear (IQueryable<recoTreeTracks> tracks, double eta, double phi, double deltaRMin2)
        {
            return tracks
                .Where(t => ROOTUtils.DeltaR2(eta, phi, t.Track_eta, t.Track_phi) < deltaRMin2)
        }
#endif
    }

}
