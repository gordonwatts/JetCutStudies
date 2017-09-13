using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToTTreeInterfacesLib;
using System.IO;

namespace TMVAUtilities
{
    /// <summary>
    /// An object that will run the TMVAReader on the fly
    /// </summary>
    public class TMVAReaderCodeGenerator<T> : IOnTheFlyCPPObject
    {
        private List<string> _variables = new List<string>();

        private FileInfo _weightFile;
        private string _methodName;

        /// <summary>
        /// Internal method to look at variables.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="weightFile"></param>
        /// <param name="useVariables"></param>
        internal TMVAReaderCodeGenerator(string methodName, FileInfo weightFile, List<string> useVariables)
        {
            _methodName = methodName;
            _weightFile = weightFile;
            _variables = useVariables;
        }

        /// <summary>
        /// Make sure the TMVAReader include file is used.
        /// </summary>
        /// <returns></returns>
        public string[] IncludeFiles()
        {
            return new string[] { "tmva/Reader.h", "vector" };
        }

        /// <summary>
        /// Generate the code we are going to need.
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public IEnumerable<string> LinesOfCode(string methodName)
        {
            // Include comments for the name of the file and its creation date. This will make sure that
            // even if nothing else changes, we will force a code update when the training re-runs.
            // (filename appears in the code, so we don't have to, but if you are looking at it, it is nice,
            // date, however is necessary).
            yield return $"// Training Weights File: {_weightFile.Name}";
            var lastWriteTime = _weightFile.LastWriteTime;
            yield return $"//   -> Modification Date      : {lastWriteTime.ToLongDateString()} {lastWriteTime.ToLongTimeString()}";
            lastWriteTime = _weightFile.LastWriteTime.ToUniversalTime();
            yield return $"//   -> Modification Date (UTC): {lastWriteTime.ToLongDateString()} {lastWriteTime.ToLongTimeString()}";

            // Static declare variables we will past to the reader.
            // This allows the nice address way of loading the reader.
            foreach (var v in _variables)
            {
                yield return $"static float {v}Unique = 0.0;";
            }

            // Perform a one-time initialization of the reader.
            yield return "static bool initUnique = false;";
            yield return "static TMVA::Reader *readerUnique = 0;";
            yield return "if (!initUnique) {";
            yield return "  initUnique = true;";
            yield return "  readerUnique = new TMVA::Reader();";

            // Declare the variables to the reader
            foreach (var v in _variables)
            {
                yield return $"  readerUnique->AddVariable(\"{v}\", &{v}Unique);";
            }

            // Book the method.
            yield return $"  readerUnique->BookMVA(\"{_methodName}\", \"{Escape(_weightFile.FullName)}\");";
            yield return "}";

            // Next, set the values of the arguments we want to evaluate.
            foreach (var v in _variables.Zip(Enumerable.Range(1, _variables.Count), (v,c) => Tuple.Create(v,c)))
            {
                yield return $"{v.Item1}Unique = v{v.Item2};";
            }

            // Finally, we can calculate the result!
            if (methodName == "MVAResultValue")
            {
                yield return $"MVAResultValue = readerUnique->EvaluateMVA(\"{_methodName}\");";
            } else
            {
                // Get back a vector.
                yield return $"MVAMultiResultValue = readerUnique->EvaluateMulticlass(\"{_methodName}\");";
            }
        }

        /// <summary>
        /// Helper function for excaping filename paths before putting them into C++ code.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string Escape(string s)
        {
            return s.Replace("\\", "\\\\");
        }

        /// <summary>
        /// This is the code that returns the value of the MVA given the arguments.
        /// </summary>
        /// <param name="v1"></param>
        /// <returns></returns>
        public double MVAResultValue (double v1)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue (double v1, double v2)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13, double v14)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13, double v14, double v15)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public double MVAResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13, double v14, double v15, double v16)
        {
            throw new InvalidOperationException("This should never be called");
        }

        public float[] MVAMultiResultValue(double v1)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13, double v14)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13, double v14, double v15)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13, double v14, double v15, double v16)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13, double v14, double v15, double v16, double v17)
        {
            throw new InvalidOperationException("This should never be called");
        }
        public float[] MVAMultiResultValue(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13, double v14, double v15, double v16, double v17, double v18)
        {
            throw new InvalidOperationException("This should never be called");
        }
    }
}
