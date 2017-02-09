using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ROOTNET.Interface.NTMVA;
using ROOTNET.NTMVA;
using static System.Tuple;
using System.IO;

namespace TMVAUtilities
{
    /// <summary>
    /// A training method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Method<T>
    {
        /// <summary>
        /// The algorithm of this method
        /// </summary>
        public ROOTNET.Interface.NTMVA.NTypes.EMVA What
        {
            get; private set;
        }

        /// <summary>
        /// The title for this run.
        /// </summary>
        public string Name
        {
            get; private set;
        }

        /// <summary>
        /// Return the weight xml file
        /// </summary>
        public FileInfo WeightFile
        {
            get
            {
                return new FileInfo(Path.Combine(new DirectoryInfo("weights").FullName, $"{Training.JobName}_{Name}.weights.xml"));
            }
        }

        /// <summary>
        /// What training are we part of?
        /// </summary>
        public Training<T> Training { get; private set; }

        /// <summary>
        /// Can only be created as part of a training, so this is hidden
        /// from everyone outside.
        /// </summary>
        internal Method(ROOTNET.Interface.NTMVA.NTypes.EMVA what, string methodTitle, string methodOptions, Training<T> parent)
        {
            this.What = what;
            this.Name = methodTitle;

            if (!string.IsNullOrWhiteSpace(methodOptions))
            {
                Option(methodOptions);
            }

            Training = parent;
        }

        /// <summary>
        /// Get an expression that will evaluate the MVA in a LINQTOTTree query.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<T,double>> GetMVAValue()
        {
            return Training.GetMVAValue(Name, WeightFile);
        }

        /// <summary>
        /// Get an expression that will evaluate the MVA in a LINQTOTTree query.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<T, float[]>> GetMVAMulticlassValue()
        {
            return Training.GetMVAMulticlassValues(Name, WeightFile);
        }

        /// <summary>
        /// Track the parameter list options we are going to apply to this method.
        /// </summary>
        private List<Tuple<string, string, string>> _parameter_options = new List<Tuple<string, string, string>>();

        /// <summary>
        /// Add a parameter option - an option that is indexed to a particular parameter.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="parameter">A function that returns the parameter you want to set it in.</param>
        /// <param name="param_name">TMVA parameter name. A [i] will be appended</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns></returns>
        public Method<T> ParameterOption<U>(Expression<Func<T, U>> parameter, string param_name, string value)
        {
            _parameter_options.Add(Create(parameter.ExtractField(), param_name, value));
            return this;
        }

        private List<string> _regular_options = new List<string>();

        /// <summary>
        /// Add a method option - this is a general option.
        /// </summary>
        /// <param name="param">The name of the parameter we want to add</param>
        /// <param name="value">The value of the parameter. Leave blank and only the parameter will be used.</param>
        /// <returns></returns>
        public Method<T> Option (string param, string value = null)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _regular_options.Add($"{param}={value}");
            } else
            {
                _regular_options.Add(param);
            }
            return this;
        }

        /// <summary>
        /// Use the parameters from here to add the list.
        /// </summary>
        /// <param name="f"></param>
        internal void Book(ROOTNET.NTMVA.NFactory f, List<string> parameterNames)
        {
            f.BookMethod(What, Name.AsTS(), BuildArgumentList(parameterNames).AsTS());
        }
        
        /// <summary>
        /// What are the arguments
        /// </summary>
        /// <param name="parameterNames"></param>
        /// <returns></returns>
        public string BuildArgumentList(List<string> parameterNames)
        {
            // Build the options
            var b = new StringBuilder();
            foreach (var o in _regular_options)
            {
                if (b.Length > 0)
                {
                    b.Append(":");
                }
                b.Append(o);
            }

            // The parameter options
            foreach (var o in _parameter_options)
            {
                var idx = parameterNames.IndexOf(o.Item1);
                if (idx < 0)
                {
                    // If it is a variable we don't care about...
                    continue;
                }
                if (b.Length > 0)
                {
                    b.Append(":");
                }
                b.Append($"{o.Item2}[{idx}]={o.Item3}");
            }

            return b.ToString();
        }

        /// <summary>
        /// Dump usage information. This includes things like what the input variables are, etc., and
        /// a sample call to the TMVAReader code.
        /// </summary>
        /// <param name="outf"></param>
        public void DumpUsageInfo(StreamWriter outf)
        {
            Training.DumpUsageInfo(outf, this, WeightFile, What);
        }

    }
}