using LINQToTTreeLib.Files;
using ROOTNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// Info on a training method
        /// </summary>
        private class TrainingMethodInfo
        {
            public ROOTNET.Interface.NTMVA.NTypes.EMVA _what;
            public string _title;
            public string _options;
        }

        private List<TrainingMethodInfo> _methods = new List<TrainingMethodInfo>();

        /// <summary>
        /// Setup a training method.
        /// </summary>
        /// <param name="what"></param>
        /// <param name="methodTitle"></param>
        /// <param name="methodOptions"></param>
        /// <returns></returns>
        public Training<T> BookMethod(ROOTNET.Interface.NTMVA.NTypes.EMVA what, string methodTitle, string methodOptions = "")
        {
            _methods.Add(new TrainingMethodInfo()
            {
                _options = methodOptions,
                _what = what,
                _title = methodTitle
            });
            return this;
        }

        public Training<T> Train(string jobName)
        {
            // THis is the file where most of the basic results from the training will be written.
            var output = NTFile.Open($"{jobName}.training.root", "RECREATE");
            try {

                // Create the factory.
                var f = new ROOTNET.NTMVA.NFactory(jobName.AsTS(), output, "!V:DrawProgressBar=True:!Silent:AnalysisType=Classification".AsTS());

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
                foreach (var field in typeof(T).GetFields())
                {
                    f.AddVariable(field.Name.AsTS());
                }

                // Now book all the methods that were requested
                foreach (var m in _methods)
                {
                    f.BookMethod(m._what, m._title.AsTS(), m._options.AsTS());
                }

                // Finally, do the training.
                f.TrainAllMethods();

                // And do the evaluation, etc.
                f.TestAllMethods();
                f.EvaluateAllMethods();

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
