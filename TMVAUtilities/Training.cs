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
        { }

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
        /// Add a singal guy to the list we are looking at.
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
            var m = new Method<T>(what, methodTitle, methodOptions);
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
        public Training<T> Train(string jobName)
        {
            // First task is to get the dates of all the files so we can see if there
            // have been updates since the last time the training ran.
            var outputFile = new FileInfo($"{jobName}.training.root");
            var hashFileName = outputFile.FullName.Replace(".root", ".hash.txt");
            var hashFile = new FileInfo(hashFileName);

            var signals = _signals.Select(s => s._sample.ToTTreeAndFile(s._title)).ToArray();
            var backgrounds = _backgrounds.Select(s => s._sample.ToTTreeAndFile(s._title)).ToArray();

            var oldestInput = signals.Concat(backgrounds).Select(i => i.Item2.LastWriteTime).Max();
            bool rerun = false;
            rerun = oldestInput > outputFile.LastWriteTime;

            // We need the list of parameters for the next step
            var parameters_names = new List<string>();
            foreach (var field in typeof(T).GetFields())
            {
                var name = field.Name;
                if (_use_variables.Count == 0 || (_use_variables.Contains(name)))
                {
                    if (!_ignore_variables.Contains(name))
                    {
                        parameters_names.Add(name);
                    }
                }
            }

            // Did the options change?
            int hash = 0;
            if (!rerun)
            {
                var bld = new StringBuilder();
                bld.Append(_tmva_options);
                foreach (var item in _ignore_variables.Concat(_use_variables))
                {
                    bld.Append(item);
                }
                foreach (var m in _methods)
                {
                    bld.Append(m.Name);
                    bld.Append(m.What.ToString());
                    bld.Append(m.BuildArgumentList(parameters_names));
                }

                hash = bld.ToString().GetHashCode();
                rerun = true;
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
            }

            if (!rerun)
            {
                return this;
            }

            // THis is the file where most of the basic results from the training will be written.
            var output = NTFile.Open(outputFile.FullName, "RECREATE");
            try {

                // Create the factory.
                var f = new ROOTNET.NTMVA.NFactory(jobName.AsTS(), output, _tmva_options.AsTS());

                // Add signal and background. Each one has to be written out.
                foreach (var sample in _signals.Select(s => s._sample.ToTTree(s._title)))
                {
                    f.AddSignalTree(sample);
                }
                foreach (var sample in _backgrounds.Select(s => s._sample.ToTTree(s._title)))
                {
                    f.AddBackgroundTree(sample);
                }

                // Do the variables by looking through each item in object T.
                // Use the windowing requests from the user.
                foreach (var n in parameters_names)
                {
                    f.AddVariable(n.AsTS());
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

                return this;
            } finally
            {
                output.Close();
            }
        }
    }

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
