using LINQToTTreeLib.CodeAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    [CPPHelperClass]
    public static class ROOTUtils
    {
        [CPPCode(IncludeFiles = new[] { "TLorentzVector.h", "TVector2.h" },
            Code = new[] {
                "double detaUnique = eta1 - eta2;",
                "double dphiUnique = TVector2::Phi_mpi_pi(phi1 - phi2);",
                "DeltaR2 = detaUnique*detaUnique + dphiUnique*dphiUnique;"
            })]
        public static double DeltaR2(double eta1, double phi1, double eta2, double phi2)
        {
            throw new NotImplementedException("this should never get called");
#if false
            double deta = eta1 - eta2;
            double deltaphi = ROOTNET.NTVector2.Phi_mpi_pi(phi1 - phi2);
            return deta * deta + deltaphi * deltaphi;
#endif
        }

        [CPPCode(IncludeFiles = new[] {"TVector2.h" },
            Code = new[] {
                "DeltaPhi = TVector2::Phi_mpi_pi(phi1 - phi2);",
            })]
        public static double DeltaPhi(double phi1, double phi2)
        {
            throw new NotImplementedException("this should never get called");
#if false
            return ROOTNET.NTVector2.Phi_mpi_pi(phi1 - phi2);
#endif
        }
    }
}
