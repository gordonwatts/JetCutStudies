using LINQToTTreeLib.CodeAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleJetCutTraining
{
    /// <summary>
    /// Temp methods to run our reader until we get something that actually works.
    /// </summary>
    [CPPHelperClass]
    public static class TMVAReaders
    {
#if false
        [CPPCode(IncludeFiles = new[] { "tmva/Reader.h"},
            Code = new[] {
                "vector<double> dataUnique;",
                "dataUnique.push_back(a1);",
                "dataUnique.push_back(a2);",
                "TMVASelectorSimpleCuts = reader->EvaluateMVA(dataUnique, \"SimpleCuts\", aux) > 0 ? true : false;"
            })]
        public static bool TMVASelectorSimpleCuts (ROOTNET.NTMVA.NReader reader, double a1, double a2, double aux)
        {
            throw new NotImplementedException("THis should never get called!");
        }
#endif
        [CPPCode(IncludeFiles = new[] { "tmva/Reader.h" },
            Code = new[] {
                "static bool initUnique = false;",
                "static float logRUnique = 0.0;",
                "static float nTrackUnique = 0.0;",
                "static TMVA::Reader *readerUnique = 0;",
                "if (!initUnique) {",
                "  initUnique = true;",
                "  readerUnique = new TMVA::Reader();",
                "  readerUnique->AddVariable(\"logR\", &logRUnique);",
                "  readerUnique->AddVariable(\"nTracks\", &nTrackUnique);",
                "  readerUnique->BookMVA(\"SimpleCuts\", \"C:\\\\Users\\\\gordo\\\\Documents\\\\Code\\\\calratio2015\\\\JetCutStudies\\\\SimpleJetCutTraining\\\\bin\\\\x86\\\\Debug\\\\weights\\\\VerySimpleTraining_SimpleCuts.weights.xml\");",
                "}",
                "logRUnique = vlogR;",
                "nTrackUnique = vnTracks;",
                "TMVASelectorSimpleCuts = readerUnique->EvaluateMVA(\"SimpleCuts\", aux) > 0 ? 1.0 : 0.0;"
            })]
        public static double TMVASelectorSimpleCuts(double vlogR, double vnTracks, double aux)
        {
            throw new NotImplementedException("THis should never get called!");
        }

        [CPPCode(IncludeFiles = new[] { "tmva/Reader.h" },
            Code = new[] {
                "static bool initUnique = false;",
                "static float logRUnique = 0.0;",
                "static float nTrackUnique = 0.0;",
                "static TMVA::Reader *readerUnique = 0;",
                "if (!initUnique) {",
                "  initUnique = true;",
                "  readerUnique = new TMVA::Reader();",
                "  readerUnique->AddVariable(\"logR\", &logRUnique);",
                "  readerUnique->AddVariable(\"nTracks\", &nTrackUnique);",
                "  readerUnique->BookMVA(\"SimpleBDT\", \"C:\\\\Users\\\\gordo\\\\Documents\\\\Code\\\\calratio2015\\\\JetCutStudies\\\\SimpleJetCutTraining\\\\bin\\\\x86\\\\Debug\\\\weights\\\\VerySimpleTraining_SimpleBDT.weights.xml\");",
                "}",
                "logRUnique = vlogR;",
                "nTrackUnique = vnTracks;",
                "TMVASelectorSimpleBDT = readerUnique->EvaluateMVA(\"SimpleBDT\");"
            })]
        public static double TMVASelectorSimpleBDT(double vlogR, double vnTracks)
        {
            throw new NotImplementedException("THis should never get called!");
        }
#if false
        var r = new ROOTNET.NTMVA.NReader();
        var s = new FileInfo("C:\\Users\\gordo\\Documents\\Code\\calratio2015\\JetCutStudies\\SimpleJetCutTraining\\bin\\x86\\Debug\\weights\\VerySimpleTraining_SimpleCuts.weights.xml");
        float[] logR = new float[2];
        float[] nTracks = new float[2];
        r.AddVariable("logR".AsTS(), logR);
            r.AddVariable("nTracks".AsTS(), nTracks);
            r.BookMVA("SimpleCuts".AsTS(), s.FullName.AsTS());
            Expression<Func<TrainingData, bool>> simpleCutsReader = tv => TMVAReaders.TMVASelectorSimpleCuts(r, tv.logR, tv.lowestPtTrack, stdCutVale.Value);
#endif
    }
}
