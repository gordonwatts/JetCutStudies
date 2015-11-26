using libDataAccess;
using LINQToTreeHelpers;
using LINQToTreeHelpers.FutureUtils;
using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLPInvestigations
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Finding the files");

            var llp = Files.Get600pi100();

            using (var outputHistograms = new FutureTFile("LLPInvestigations.root"))
            {
                // How often does a LLP get shared?

#if false
                var sharedJets = from ev in llp
                                  from j1 in ev.Jets
                                          from j2 in ev.Jets
                                          where j1.LLP.IsGoodIndex() && j2.LLP.IsGoodIndex()
                                          where j1 != j2
                                          where j1.LLP == j2.LLP
                                          select Tuple.Create(j1, j2);
#endif

                var sharedJets = from ev in llp
                                 from j1 in ev.Jets
                                 from j2 in ev.Jets
                                 where j1.LLP.IsGoodIndex() && j2.LLP.IsGoodIndex()
                                 where j1 != j2
                                 where j1.LLP.Lxy == j2.LLP.Lxy
                                 select Tuple.Create(j1, j2);

                var count = sharedJets.FutureCount();
                Console.WriteLine($"Number of jets that share an LLP: {count.Value}");

            }
        }
    }
}
