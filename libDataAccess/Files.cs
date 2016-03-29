using DiVertAnalysis;
using LINQToTTreeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess
{
    /// <summary>
    /// Centralize the files we are accessing so multiple programs don't have to keep re-writing stuff.
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// Default setting for # of files to fetch when we run. 0 means we are running on the full data sample.
        /// </summary>
        public static int NFiles = 1;

        /// <summary>
        /// Set to true to get a complete dump of what is going on during grid file access.
        /// </summary>
        public static bool VerboseFileFetch = false;

        /// <summary>
        /// Return a dataset list given the name of the dataset.
        /// </summary>
        /// <param name="dsname"></param>
        /// <returns></returns>
        private static FileInfo[] GetFileList(string dsname)
        {
            TraceListener listener = null;

            if (VerboseFileFetch)
            {
                listener = new TextWriterTraceListener(Console.Out);
                Trace.Listeners.Add(listener);
            }

            try {
                return GRIDJobs.FindJobFiles("DiVertAnalysis",
                    4,
                    dsname,
                    nFiles: NFiles,
                    statusUpdate: l => Console.WriteLine(l),
                    intelligentLocal: true);
            } finally
            {
                if (listener != null)
                {
                    Trace.Listeners.Remove(listener);
                }
            }
        }

        /// <summary>
        /// Get the J2Z files for running.
        /// </summary>
        /// <returns></returns>
        public static QueriableTTree<recoTree> GetJ1Z()
        {
            var backgroundFiles = GetFileList("mc15_13TeV.361021.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ1W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452");
            var backgroundEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFiles);
            //backgroundEvents.IgnoreQueryCache = true;
            //backgroundEvents.UseStatementOptimizer = false;
            return backgroundEvents;
        }

        public static QueriableTTree<recoTree> GetJ2Z()
        {
            var backgroundFiles = GetFileList("mc15_13TeV.361022.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ2W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452");
            var backgroundEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFiles);
            //backgroundEvents.IgnoreQueryCache = true;
            //backgroundEvents.UseStatementOptimizer = false;
            return backgroundEvents;
        }

        public static QueriableTTree<recoTree> GetJ3Z()
        {
            var backgroundFiles = GetFileList("mc15_13TeV.361023.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ3W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452");
            var backgroundEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFiles);
            //backgroundEvents.IgnoreQueryCache = true;
            //backgroundEvents.UseStatementOptimizer = false;
            return backgroundEvents;
        }

        public static QueriableTTree<recoTree> GetJ4Z()
        {
            var backgroundFiles = GetFileList("mc15_13TeV.361024.Pythia8EvtGen_A14NNPDF23LO_jetjet_JZ4W.merge.DAOD_EXOT15.e3668_s2576_s2132_r6765_r6282_p2452");
            var backgroundEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(backgroundFiles);
            //backgroundEvents.IgnoreQueryCache = true;
            //backgroundEvents.UseStatementOptimizer = false;
            return backgroundEvents;
        }

        /// <summary>
        /// Metadata we hold for each sample
        /// </summary>
        public class MetaData
        {
            public recoTree Data;
            public double xSectionWeight;
        }

        /// <summary>
        /// Gets properly weighted background sample as one.
        /// </summary>
        /// <returns></returns>
        public static IQueryable<MetaData> GetAllJetSamples()
        {
            const int nEvents = 20000;
            return
                GenerateStream(libDataAccess.Files.GetJ2Z().Take(nEvents), 1.0)
                .Concat(GenerateStream(libDataAccess.Files.GetJ3Z().Take(nEvents), 1.0))
                .Concat(GenerateStream(libDataAccess.Files.GetJ4Z().Take(nEvents), 1.0));
                ;
        }

        /// <summary>
        /// Return a metadata stream version of the event sequence.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="xSecWeight"></param>
        /// <returns></returns>
        public static IQueryable<MetaData> GenerateStream(this IQueryable<recoTree> source, double xSecWeight)
        {
            return source.Select(e => new MetaData() { Data = e, xSectionWeight = xSecWeight });
        }

#if false
        public static QueriableTTree<recoTree> Get125pi15()
        {
            var sig = GetFileList("user.hrussell.mc15_13TeV.301303.HSS_mH125mS15.reco.s2698_r7144_EXT2");
            var sigEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(sig);
            //sigEvents.IgnoreQueryCache = true;
            //sigEvents.UseStatementOptimizer = false;
            return sigEvents;
        }

        public static QueriableTTree<recoTree> Get125pi40()
        {
            var sig = GetFileList("user.hrussell.mc15_13TeV.301298.HSS_mH125mS40.reco_20k.s2698_r7144_v03_EXT2");
            var sigEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(sig);
            //sigEvents.IgnoreQueryCache = true;
            //sigEvents.UseStatementOptimizer = false;
            return sigEvents;
        }

        public static QueriableTTree<recoTree> Get600pi100()
        {
            var sig = GetFileList("user.hrussell.mc15_13TeV.301301.HSS_mH600mS100.reco_20k.s2698_r7144_v03_EXT2");
            var sigEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(sig);
            //sigEvents.IgnoreQueryCache = true;
            //sigEvents.UseStatementOptimizer = false;
            return sigEvents;
        }
#endif
        public static QueriableTTree<recoTree> Get200pi25lt5m()
        {
            var sig = GetFileList("mc15_13TeV.304805.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH200_mS25_lt5m.merge.AOD.e4754_s2698_r7146_r6282");
            var sigEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(sig);
            //sigEvents.IgnoreQueryCache = true;
            //sigEvents.UseStatementOptimizer = false;
            return sigEvents;
        }

        public static QueriableTTree<recoTree> Get400pi100lt9m()
        {
            var sig = GetFileList("mc15_13TeV.304813.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH400_mS100_lt9m.merge.AOD.e4754_s2698_r7146_r6282");
            var sigEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(sig);
            //sigEvents.IgnoreQueryCache = true;
            //sigEvents.UseStatementOptimizer = false;
            return sigEvents;
        }

        public static QueriableTTree<recoTree> Get600pi150lt9m()
        {
            var sig = GetFileList("mc15_13TeV.304817.MadGraphPythia8EvtGen_A14NNPDF23LO_HSS_LLP_mH600_mS150_lt9m.merge.AOD.e4754_s2698_r7146_r6282");
            var sigEvents = DiVertAnalysis.QueryablerecoTree.CreateQueriable(sig);
            //sigEvents.IgnoreQueryCache = true;
            //sigEvents.UseStatementOptimizer = false;
            return sigEvents;
        }
    }
}
