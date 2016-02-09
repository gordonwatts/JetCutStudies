﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ROOTNET.Interface.NTMVA;
using ROOTNET.NTMVA;
using static System.Tuple;

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
        private ROOTNET.Interface.NTMVA.NTypes.EMVA _what;

        /// <summary>
        /// The title for this run.
        /// </summary>
        private string _methodTitle;

        /// <summary>
        /// Can only be created as part of a training, so this is hidden
        /// from everyone outside.
        /// </summary>
        internal Method(ROOTNET.Interface.NTMVA.NTypes.EMVA what, string methodTitle, string methodOptions)
        {
            this._what = what;
            this._methodTitle = methodTitle;

            if (!string.IsNullOrWhiteSpace(methodOptions))
            {
                Option(methodOptions);
            }
        }

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
                if (b.Length > 0)
                {
                    b.Append(":");
                }
                b.Append($"{o.Item2}[{idx}]={o.Item3}");
            }

            // And do the booking
            f.BookMethod(_what, _methodTitle.AsTS(), b.ToString().AsTS());
        }
    }
}
