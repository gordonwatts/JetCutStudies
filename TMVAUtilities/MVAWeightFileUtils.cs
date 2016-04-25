using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace TMVAUtilities
{
    public static class MVAWeightFileUtils
    {
        /// <summary>
        /// Given a weight file and a type of the training tree used to train it, hook up
        /// everything needed to get the MVA result from TMVA.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Expression<Func<T, double>> MVAFromWeightFile<T> (FileInfo file)
        {
            // We need to extract a bunch of information from the XML file
            // in order to proceed.
            var info = ParseTMVAXMLFile(file);

            // Now we can generate the appropriate object that will emit C++ code.
            var code = new TMVAReaderCodeGenerator<T>(info.Name, file, info.Variables);

            // And, finally, the code to generate the expression that maps T onto the list of variables.
            return ExpressionUtils.BuildLambdaExpression(info.Variables, code);
        }

        private class TMVAAlgInfo
        {
            /// <summary>
            /// The name used on the BDT
            /// </summary>
            public string Name;

            /// <summary>
            /// An in order list of the variable names used in the training.
            /// </summary>
            public List<string> Variables;
        }

        /// <summary>
        /// Parse the XML file for everything we need.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static TMVAAlgInfo ParseTMVAXMLFile(FileInfo file)
        {
            // Parse with a reader to make ourselves efficient.
            var rdr = XmlReader.Create(file.FullName);

            int gotitAll = 2;
            var result = new TMVAAlgInfo();

            while (gotitAll != 0)
            {
                if (!rdr.Read())
                {
                    throw new InvalidDataException($"Weights file {file.FullName} can't be parsed");
                }
                if (rdr.NodeType == XmlNodeType.Element)
                {
                    if (rdr.Name == "MethodSetup")
                    {
                        result.Name = ParseMethodSetup(rdr);
                        gotitAll--;
                    }
                    if (rdr.Name == "Variables")
                    {
                        gotitAll--;
                        result.Variables = ParseVariableList(rdr);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extract the variable names
        /// </summary>
        /// <param name="rdr"></param>
        /// <returns></returns>
        private static List<string> ParseVariableList(XmlReader rdr)
        {
            List<string> vars = new List<string>();

            while (true)
            {
                rdr.Read();
                rdr.MoveToContent();
                if (rdr.Name != "Variable")
                {
                    return vars;
                }
                rdr.MoveToAttribute("Expression");
                vars.Add(rdr.Value);
            }
        }

        /// <summary>
        /// Get the name.
        /// </summary>
        /// <param name="rdr"></param>
        /// <returns></returns>
        private static string ParseMethodSetup(XmlReader rdr)
        {
            if (!rdr.MoveToAttribute("Method"))
            {
                throw new InvalidDataException($"Unable to find Method attribute");
            }
            return rdr.Value.Substring(0, rdr.Value.IndexOf("::"));
        }
    }
}
