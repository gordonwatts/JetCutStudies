using LINQToTTreeLib.Files;
using ROOTNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TMVAUtilities
{
    /// <summary>
    /// Helper class for doing the training
    /// </summary>
    public class Training<T>
    {
        
        /// <summary>
        /// Make it hard to create this object without using the Signal or Background helper.
        /// </summary>
        internal Training()
        {
            JobName = "";
        }

        /// <summary>
        /// Hold all info for a sample.
        /// </summary>
        class SampleInfo
        {
            public string _title;
            public IQueryable<T> _sample;
        }

        private List<SampleInfo> _signals = new List<SampleInfo>();
        private List<SampleInfo> _backgrounds = new List<SampleInfo>();

        private List<string> _ignore_variables = new List<string>();
        private List<string> _use_variables = new List<string>();

        /// <summary>
        /// Job name we ran under. To help with file names.
        /// </summary>
        public string JobName { get; private set; }

        /// <summary>
        /// List the variables to be used for the training. Only these will be used.
        /// If this isn't called, then everything will be used (minus the ignored).
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="vars"></param>
        /// <returns></returns>
        public Training<T> UseVariables<U>(params Expression<Func<T,U>>[] vars)
        {
            foreach (var v in vars)
            {
                _use_variables.Add(v.ExtractField());
            }
            return this;
        }

        /// <summary>
        /// Build an expression that can be used in a LINQ query to evaluate this guy given the input
        /// we are running on.
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        internal Expression<Func<T, double>> GetMVAValue(string methodName, FileInfo weightFile)
        {
            // Get the list of variables we are going to need to pass in.

            // The center of it all is the code statement that will run everything given a list of inputs.
            var pnames = GetParameterAndWeightNames().Item2;
            var code = new TMVAReaderCodeGenerator<T>(methodName, weightFile, pnames);

            // We are going to build up a lambda expression that takes T as an argument and returns a
            // double.
            Expression<Func<T, double>> r = myT => code.MVAResultValue(1.0);

            var myTArgParam = Expression.Parameter(typeof(T));
            var args = pnames
                .Select(p => typeof(T).GetField(p))
                .Select(pf => Expression.Field(myTArgParam, pf))
                .Select(fa => (fa.Type != typeof(double)) ? Expression.Convert(fa, typeof(double)) as Expression : fa);

            var argTypes = pnames.Select(_ => typeof(double)).ToArray();
            var method = code.GetType().GetMethod("MVAResultValue", argTypes);
            var call = Expression.Call(Expression.Constant(code), method, args.ToArray());

            var lambda = Expression.Lambda<Func<T, double>>(call, myTArgParam);

            return lambda;

            //return r;
        }

        /// <summary>
        /// Ignore the variables listed. Overrides the UseVariables call.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="vars"></param>
        /// <returns></returns>
        public Training<T> IgnoreVariables<U>(params Expression<Func<T,U>>[] vars)
        {
            foreach (var v in vars)
            {
                _ignore_variables.Add(v.ExtractField());
            }
            return this;
        }


        /// <summary>
        /// Add a background to our list of sources.
        /// </summary>
        /// <param name="source"></param>
        public Training<T> Background(IQueryable<T> source, string title = "")
        {
            _backgrounds.Add(new SampleInfo() { _title = title, _sample = source });
            return this;
        }

        /// <summary>
        /// Add a signal guy to the list we are looking at.
        /// </summary>
        /// <param name="source"></param>
        public Training<T> Signal(IQueryable<T> source, string title = "")
        {
            _signals.Add(new SampleInfo() { _title = title, _sample = source });
            return this;
        }

        /// <summary>
        /// List of methods we are going to train agains.
        /// </summary>
        private List<Method<T>> _methods = new List<Method<T>>();

        /// <summary>
        /// Setup a training method.
        /// </summary>
        /// <param name="what"></param>
        /// <param name="methodTitle"></param>
        /// <param name="methodOptions"></param>
        /// <returns></returns>
        public Method<T> AddMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA what, string methodTitle, string methodOptions = "")
        {
            var m = new Method<T>(what, methodTitle, methodOptions, this);
            _methods.Add(m);
            return m;
        }

        /// <summary>
        /// Global tmva options for factory creation.
        /// </summary>
        private string _tmva_options = "!V:DrawProgressBar=True:!Silent:AnalysisType=Classification";

        /// <summary>
        /// Run the training
        /// </summary>
        /// <param name="jobName"></param>
        /// <returns></returns>
        public TrainingResult<T> Train(string jobName)
        {
            if (!string.IsNullOrWhiteSpace(JobName))
            {
                throw new InvalidOperationException("Can't train twice!");
            }

            var signals = _signals.SelectMany(s => s._sample.ToTTreeAndFile(s._title)).ToArray();
            var backgrounds = _backgrounds.SelectMany(s => s._sample.ToTTreeAndFile(s._title)).ToArray();

            var oldestInput = signals.Concat(backgrounds).Select(i => i.Item2.LastWriteTime).Max();

            // We need an ordered list of parameters for the next step
            var r = GetParameterAndWeightNames();
            var weight_name = r.Item1;
            var parameters_names = r.Item2;

            // Did the options change? Calc a string for the hash.
            var bldOptionsString = new StringBuilder();
            bldOptionsString.Append(_tmva_options);
            foreach (var item in _ignore_variables.Concat(_use_variables))
            {
                bldOptionsString.Append(item);
            }
            foreach (var item in _use_variables)
            {
                bldOptionsString.Append(item);
            }

            // How about the parameters?
            foreach (var p in parameters_names)
            {
                bldOptionsString.Append(p);
            }
            bldOptionsString.Append(weight_name);

            // Methods and their options.
            foreach (var m in _methods)
            {
                bldOptionsString.Append(m.Name);
                bldOptionsString.Append(m.What.ToString());
                bldOptionsString.Append(m.BuildArgumentList(parameters_names));
            }

            // signal and background files
            foreach (var fname in signals.Concat(backgrounds).Select(s => s.Item2.FullName))
            {
                bldOptionsString.Append(fname);
            }

            // Next, given these inputs, we can calculate the names of the output files.
            var hash = bldOptionsString.ToString().GetHashCode();

            JobName = $"{jobName}-{hash}";

            var outputFile = new FileInfo($"{jobName}-{hash}.training.root");
            var hashFileName = outputFile.FullName.Replace(".root", ".hash.txt");
            var hashFile = new FileInfo(hashFileName);

            // And now see if the hash has changed since our last run as a double check
            // [note this is redundant now the hash now exists in filenames that are output
            // by the training.]
            bool rerun = true;
            if (hashFile.Exists)
            {
                using (var rdr = hashFile.OpenText())
                {
                    var s = rdr.ReadLine();
                    if (s == hash.ToString())
                    {
                        rerun = false;
                    }
                }
            }

            rerun = rerun || oldestInput > outputFile.LastWriteTime;
            rerun = rerun || !hashFile.Exists;

            foreach (var m in _methods)
            {
                rerun = rerun || !m.WeightFile.Exists;
            }

            // Build the result object. If there is no need to re-run, then
            // we can ignore this.
            var resultObject = new TrainingResult<T>()
            {
                OutputName = new DirectoryInfo("weights"),
                JobName = $"{jobName}-{hash}",
                TrainingOutputFile = outputFile,
                MethodList = _methods.ToArray(),
            };

            if (!rerun)
            {
                return resultObject;
            }

            // This is the file where most of the basic results from the training will be written.
            var output = NTFile.Open(outputFile.FullName, "RECREATE");
            try
            {

                // Create the factory.
                var f = new ROOTNET.NTMVA.NFactory($"{jobName}-{hash}".AsTS(), output, _tmva_options.AsTS());

                // Add signal and background. Each one has to be written out.
                foreach (var sample in signals)
                {
                    f.AddSignalTree(sample.Item1);
                }
                foreach (var sample in backgrounds)
                {
                    f.AddBackgroundTree(sample.Item1);
                }

                // Do the variables by looking through each item in object T.
                // Use the windowing requests from the user.
                foreach (var n in parameters_names)
                {
                    f.AddVariable(n.AsTS());
                }

                // The weight
                if (!string.IsNullOrWhiteSpace(weight_name))
                {
                    f.WeightExpression = weight_name.AsTS();
                }

                // Now book all the methods that were requested
                foreach (var m in _methods)
                {
                    m.Book(f, parameters_names);
                }

                // Finally, do the training.
                f.TrainAllMethods();

                // And do the evaluation, etc.
                f.TestAllMethods();
                f.EvaluateAllMethods();

                // Write out the hash value
                using (var wr = hashFile.CreateText())
                {
                    wr.WriteLine(hash);
                }

                return resultObject;
            }
            finally
            {
                output.Close();
            }
        }

        /// <summary>
        /// Get the list of parameter names to use from the object T's fields.
        /// </summary>
        /// <param name="parameters_names"></param>
        /// <returns></returns>
        private Tuple<string, List<string>> GetParameterAndWeightNames()
        {
            var parameter_names = new List<string>();
            string weight_name = "";
            foreach (var field in typeof(T).GetFields().OrderBy(f => f.MetadataToken))
            {
                var name = field.Name;
                if (_use_variables.Count == 0 || (_use_variables.Contains(name)))
                {
                    if (!_ignore_variables.Contains(name))
                    {
                        if (name == "Weight")
                        {
                            weight_name = name;
                        }
                        else
                        {
                            parameter_names.Add(name);
                        }
                    }
                }
            }

            return Tuple.Create(weight_name, parameter_names);
        }
    }

    /// <summary>
    /// Some helpers to get the training environment created.
    /// </summary>
    public static class TrainingHelpers
    {
        /// <summary>
        /// Create a training environment based on a source with a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Training<T> AsSignal<T>(this IQueryable<T> source, string title = "")
        {
            var t = new Training<T>();
            t.Signal(source, title);
            return t;
        }

        /// <summary>
        /// Create a training environment based on this background
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Training<T> AsBackground<T>(this IQueryable<T> source, string title = "")
        {
            var t = new Training<T>();
            t.Background(source, title);
            return t;
        }
    }
}
