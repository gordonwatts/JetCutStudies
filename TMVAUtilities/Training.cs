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

        private List<IQueryable<T>> _signals = new List<IQueryable<T>>();
        private List<IQueryable<T>> _backgrounds = new List<IQueryable<T>>();

        /// <summary>
        /// Add a background to our list of sources.
        /// </summary>
        /// <param name="source"></param>
        public Training<T> Background(IQueryable<T> source)
        {
            _backgrounds.Add(source);
            return this;
        }

        /// <summary>
        /// Add a singal guy to the list we are looking at.
        /// </summary>
        /// <param name="source"></param>
        public Training<T> Signal(IQueryable<T> source)
        {
            _signals.Add(source);
            return this;
        }

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
                var f = new ROOTNET.NTMVA.NFactory(jobName.AsTS(), output);

                // Add signal and background. Each one has to be written out.
                foreach (var sample in _signals.Select(s => s.ToTTree()))
                {
                    f.AddSignalTree(sample);
                }
                foreach (var sample in _backgrounds.Select(s => s.ToTTree()))
                {
                    f.AddBackgroundTree(sample);
                }

                // Now book all the methods that were requested
                foreach (var m in _methods)
                {
                    f.BookMethod(m._what, m._title.AsTS(), m._options.AsTS());
                }

                // Do the variables by looking through each item in object T.
                foreach (var field in typeof(T).GetFields())
                {
                    f.AddVariable(field.Name.AsTS());
                }

                // Finally, do the training.
                f.TrainAllMethods();

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
        public static Training<T> AsSignal<T>(this IQueryable<T> source)
        {
            var t = new Training<T>();
            t.Signal(source);
            return t;
        }

        /// <summary>
        /// Create a training environment based on this background
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Training<T> AsBackground<T>(this IQueryable<T> source)
        {
            var t = new Training<T>();
            t.Background(source);
            return t;
        }
    }
}
