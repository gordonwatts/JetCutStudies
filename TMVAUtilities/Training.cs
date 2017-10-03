using LINQToTTreeLib;
using LINQToTTreeLib.ExecutionCommon;
using LINQToTTreeLib.Files;
using LINQToTTreeLib.Variables;
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
            public Expression<Func<T, bool>> _isTrainingEvent;
            public string _eventClass;
        }

        private List<SampleInfo> _trainingSamples = new List<SampleInfo>();

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
            // The center of it all is the code statement that will run everything given a list of inputs.
            var pnames = GetParameterAndWeightNames().Item2;
            var code = new TMVAReaderCodeGenerator<T>(methodName, weightFile, pnames);

            // We are going to build up a lambda expression that takes T as an argument and returns a
            // double.
            return ExpressionUtils.BuildLambdaExpression(pnames, code);
        }

        internal Expression<Func<T,float[]>> GetMVAMulticlassValues(string methodName, FileInfo weightFile)
        {
            // The center of it all is the code statement that will run everything given a list of inputs.
            var pnames = GetParameterAndWeightNames().Item2;
            var code = new TMVAReaderCodeGenerator<T>(methodName, weightFile, pnames);

            // We are going to build up a lambda expression that takes T as an argument and returns a
            // double.
            return ExpressionUtils.BuildMulticlassLambdaExpression(pnames, code);
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
        /// Add a new event class to the training.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="entClass"></param>
        /// <param name="isTrainingEvent"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public Training<T> EventClass(IQueryable<T> source, string entClass, Expression<Func<T, bool>> isTrainingEvent = null, string title = null)
        {
            if (_classificationType != ClassificationType.Undetermined && _classificationType != ClassificationType.MultiClass)
            {
                throw new InvalidOperationException("Can't add a class sample to a training that is using Signal and Background");
            }
            _trainingSamples.Add(new SampleInfo() { _title = title, _eventClass = entClass, _sample = source, _isTrainingEvent = isTrainingEvent });
            _classificationType = ClassificationType.MultiClass;

            return this; 
        }

        /// <summary>
        /// Add a background to our list of sources.
        /// </summary>
        /// <param name="source"></param>
        public Training<T> Background(IQueryable<T> source, string title = "", Expression<Func<T, bool>> isTrainingEvent = null)
        {
            if (_classificationType != ClassificationType.Undetermined && _classificationType != ClassificationType.SignalBackground)
            {
                throw new InvalidOperationException("Background sample can't be added to a training that is using multple event classes");
            }

            // Make sure the user isn't adding a selection when no other is present. We have to have
            // the same added all the time.
            if (isTrainingEvent == null && Backgrounds().Where(b => b._isTrainingEvent != null).Any())
            {
                throw new ArgumentException("A background sample with a training event selector has already been added - you can't add one with no training selector.");
            }
            if (isTrainingEvent != null && Backgrounds().Where(b => b._isTrainingEvent == null).Any())
            {
                throw new ArgumentException("A background sample with a training event selector has not been added - you can't add one with a training selector.");
            }
            _trainingSamples.Add(new SampleInfo() { _title = title, _eventClass = "Background", _sample = source, _isTrainingEvent = isTrainingEvent });
            _classificationType = ClassificationType.SignalBackground;
            return this;
        }

        /// <summary>
        /// Track the classifications we might be doing.
        /// </summary>
        enum ClassificationType
        {
            SignalBackground,
            MultiClass,
            Undetermined
        }

        /// <summary>
        /// Undetermined to start with, and then it settles down.
        /// </summary>
        private ClassificationType _classificationType = ClassificationType.Undetermined;

        /// <summary>
        /// Return the background samles
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SampleInfo> Backgrounds()
        {
            return _trainingSamples.Where(s => s._eventClass == "Background");
        }

        /// <summary>
        /// Return the signal samples
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SampleInfo> Signals()
        {
            return _trainingSamples.Where(s => s._eventClass == "Signal");
        }

        /// <summary>
        /// Add a signal guy to the list we are looking at.
        /// </summary>
        /// <param name="source"></param>
        public Training<T> Signal(IQueryable<T> source, string title = "", Expression<Func<T, bool>> isTrainingEvent = null)
        {
            if (_classificationType != ClassificationType.Undetermined && _classificationType != ClassificationType.SignalBackground)
            {
                throw new InvalidOperationException("Background sample can't be added to a training that is using multple event classes");
            }

            if (isTrainingEvent == null && Signals().Where(b => b._isTrainingEvent != null).Any())
            {
                throw new ArgumentException("A background sample with a training event selector has already been added - you can't add one with no training selector.");
            }
            if (isTrainingEvent != null && Signals().Where(b => b._isTrainingEvent == null).Any())
            {
                throw new ArgumentException("A background sample with a training event selector has not been added - you can't add one with a training selector.");
            }
            _trainingSamples.Add(new SampleInfo() { _title = title, _eventClass = "Signal", _sample = source, _isTrainingEvent = isTrainingEvent });
            _classificationType = ClassificationType.SignalBackground;
            return this;
        }

        /// <summary>
        /// List of methods we are going to train against.
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
            if (_classificationType == ClassificationType.Undetermined)
            {
                throw new InvalidOperationException("You can't call AddMethod before adding signal and background or class samples.");
            }

            var m = new Method<T>(what, methodTitle, methodOptions, this, _classificationType == ClassificationType.MultiClass);
            _methods.Add(m);
            return m;
        }

        /// <summary>
        /// Global tmva options for factory creation.
        /// </summary>
        private string _tmva_options = "!V:DrawProgressBar=True:!Silent";

        /// <summary>
        /// Return a list of used variable names
        /// </summary>
        /// <returns></returns>
        public string[] UsedVariables()
        {
            var p = GetParameterAndWeightNames();
            return p.Item2.ToArray();
        }

        /// <summary>
        /// Dump to a text file some code on how this should be run.
        /// </summary>
        /// <param name="outf"></param>
        /// <param name="name"></param>
        /// <param name="weightFile"></param>
        internal void DumpUsageInfo(StreamWriter outf, Method<T> method, FileInfo weightFile, ROOTNET.Interface.NTMVA.NTypes.EMVA what)
        {
            // First a simple listing of the input variables.
            var p = GetParameterAndWeightNames();
            outf.WriteLine("Input Variable Summary");
            outf.WriteLine("======================");
            foreach (var v in p.Item2.Zip(Enumerable.Range(0, p.Item2.Count), (vname, index) => Tuple.Create(index, vname)))
            {
                outf.WriteLine($"  {v.Item1}: {v.Item2}");
            }

            // Now show how to call and "init" the TMVAReader
            outf.WriteLine();
            outf.WriteLine("Calling TMVAReader");
            outf.WriteLine("======================");
            outf.WriteLine("Replace vXX with the appropriate values, and the path to the filename as needed.");
            switch (_classificationType)
            {
                case ClassificationType.SignalBackground:
                    outf.WriteLine("Return type from the EvaluateMVA is float");
                    break;
                case ClassificationType.MultiClass:
                    outf.WriteLine("Return type from the EvaluateMulticlass is vector<float>");
                    break;
                case ClassificationType.Undetermined:
                    throw new InvalidOperationException("Should never call without adding samples");
                default:
                    throw new InvalidOperationException("Should never call without adding samples");
            }
            var code = new TMVAReaderCodeGenerator<T>(method.Name, weightFile, p.Item2);
            foreach (var i in (code.IncludeFiles() == null) ? Enumerable.Empty<string>() : code.IncludeFiles())
            {
                outf.WriteLine($"#include \"{i}\"");
            }

            foreach (var l in code.LinesOfCode(method.Name))
            {
                outf.WriteLine($"  {l}");
            }

            // Next we have to write out how the variables were prepared. Lets do the background, as that is likely to
            // be most what the data preparation is going to look like.
            outf.WriteLine();
            outf.WriteLine("How the variables are calculated");
            outf.WriteLine("======================");
            var b = _trainingSamples.First();

            foreach (var pname in p.Item2)
            {
                var tParam = Expression.Parameter(typeof(T));
                var access = Expression.Field(tParam, pname);

                string pp = null;
                if (access.Type == typeof(int))
                {
                    var lambda = Expression.Lambda<Func<T, int>>(access, tParam);
                    var selector = b._sample.Select(lambda);
                    pp = selector.PrettyPrintQuery();
                } else if (access.Type == typeof(double))
                {
                    var lambda = Expression.Lambda<Func<T, double>>(access, tParam);
                    var selector = b._sample.Select(lambda);
                    pp = selector.PrettyPrintQuery();
                } else
                {
                    throw new InvalidOperationException($"Do not know how to generate an expression for type {access.Type.Name}");
                }
                outf.WriteLine($"{pname} = {pp}");
                outf.WriteLine();
            }

            // Parameters and etc for all of this.
            outf.WriteLine();
            outf.WriteLine("General Information About Training");
            outf.WriteLine("======================");
            outf.WriteLine($"Global TMVA parameters: {_tmva_options}");
            outf.WriteLine($"Method {what.ToString()} with parameters '{method.BuildArgumentList(p.Item2)}'");
            foreach (var eventClass in _trainingSamples.GroupBy(s => s._eventClass))
            {
                outf.WriteLine($"{eventClass.Key} Total Events: {eventClass.Where(ms => ms._sample != null).Select(ms => ms._sample.Count()).Sum()}");
                foreach (var s in eventClass.Where(ms => ms._sample != null).Zip(Enumerable.Range(1, eventClass.Count()), (bs, c) => Tuple.Create(bs, c)))
                {
                    var trainingEventsSelection = s.Item1._isTrainingEvent == null ? "" : $" (training events when ({s.Item1._isTrainingEvent.ToString()})";
                    outf.WriteLine($"  {eventClass.Key} input stream #{s.Item2}: {s.Item1._sample.Count()} events{trainingEventsSelection}");
                }
            }
        }

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

            // We need an ordered list of parameters for the next step
            var r = GetParameterAndWeightNames();
            var weight_name = r.Item1;
            var parameters_names = r.Item2;

            // Did the options change? Calc a string for the hash.
            var bldOptionsString = new StringBuilder();
            bldOptionsString.Append(_tmva_options);
            foreach (var item in _ignore_variables)
            {
                bldOptionsString.Append($"i-{item}");
            }
            foreach (var item in _use_variables)
            {
                bldOptionsString.Append($"u-{item}");
            }

            // How about the parameters?
            foreach (var p in parameters_names)
            {
                bldOptionsString.Append($"p-{p}");
            }
            bldOptionsString.Append(weight_name);

            // Methods and their options.
            foreach (var m in _methods)
            {
                bldOptionsString.Append(m.Name);
                bldOptionsString.Append(m.What.ToString());
                bldOptionsString.Append(m.BuildArgumentList(parameters_names));
            }

            var trainingSamples = _trainingSamples.SelectMany(s => ExtractTrainingAndTestingSamples(s)).ToArray();

            var oldestInput = trainingSamples.Select(i => i.Item2.LastWriteTime).Max();

            // signal and background files
            foreach (var fname in trainingSamples.Select(s => s.Item2.FullName))
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

            // Run the training
            TrainInBash(jobName, weight_name, parameters_names, trainingSamples, hash, outputFile, hashFile);
            return resultObject;
        }

        /// <summary>
        /// Run the training in some version of root 6 in bash
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="weight_name"></param>
        /// <param name="parameters_names"></param>
        /// <param name="trainingSamples"></param>
        /// <param name="hash"></param>
        /// <param name="outputFile"></param>
        /// <param name="hashFile"></param>
        /// <returns></returns>
        private void TrainInBash(string jobName, string weight_name, List<string> parameters_names, Tuple<ROOTNET.Interface.NTTree, FileInfo, FileTrainingType, string>[] trainingSamples, int hash, FileInfo outputFile, FileInfo hashFile)
        {
            // Write commands to a buffer that we can then write out and execute.
            var script = new StringBuilder();

            // This is the file where most of the basic results from the training will be written.
            script.AppendLine($"TFile *output = TFile::Open(\"<><>{outputFile.FullName}<><>\", \"RECREATE\");");

            // Create the factory.
            var options = _tmva_options +
                    (_classificationType == ClassificationType.SignalBackground ? ":AnalysisType=Classification" : ":AnalysisType=Multiclass");
            script.AppendLine($"TMVA::Factory *f = new TMVA::Factory(\"{jobName}-{hash}\", output, \"{options}\");");

            // Next, add the samples.
            bool isSimpleSigBack = _classificationType == ClassificationType.SignalBackground;
            int count = 0;
            foreach (var sample in trainingSamples)
            {
                // Load up the file
                script.AppendLine($"TFile *tfile_{count} = TFile::Open(\"<><>{sample.Item2}<><>\", \"READ\");");
                script.AppendLine($"TTree *t_{count} = static_cast<TTree*> (tfile_{count}->Get(\"{sample.Item1.Name}\"));");

                // How we add it depends on what it is about the sample.
                switch (sample.Item3)
                {
                    case FileTrainingType.IsTraining:
                        if (isSimpleSigBack)
                        {
                            if (sample.Item4 == "Signal")
                            {
                                script.AppendLine($"f->AddSignalTree(t_{count}, 1.0, kTraining);");
                            }
                            else
                            {
                                script.AppendLine($"f->AddBackgroundTree(t_{count}, 1.0, kTraining);");
                            }
                        }
                        else
                        {
                            script.AppendLine($"f->AddTree(t_{count}, \"{sample.Item4}\", 1.0, new TCut(\"\"), kTraining);");
                        }
                        break;
                    case FileTrainingType.IsBoth:
                        if (isSimpleSigBack)
                        {
                            if (sample.Item4 == "Signal")
                            {
                                script.AppendLine($"f->AddSignalTree(t_{count});");
                            }
                            else
                            {
                                script.AppendLine($"f->AddBackgroundTree(t_{count});");
                            }
                        }
                        else
                        {
                            script.AppendLine($"f->AddTree(t_{count}, \"{sample.Item4}\", 1.0);");
                        }
                        break;
                    case FileTrainingType.IsTesting:
                        if (isSimpleSigBack)
                        {
                            if (sample.Item4 == "Signal")
                            {
                                script.AppendLine($"f->AddSignalTree(t_{count}, 1.0, kTesting);");
                            }
                            else
                            {
                                script.AppendLine($"f->AddBackgroundTree(t_{count}, 1.0, kTesting);");
                            }
                        }
                        else
                        {
                            script.AppendLine($"f->AddTree(t_{count}, \"{sample.Item4}\", 1.0, new TCut(\"\"), kTesting);");
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Inavlid program state: Do not know how to add samples of type {sample.Item3}.");
                }
                count++;
            }

            // Do the variables by looking through each item in object T.
            // Use the windowing requests from the user.
            foreach (var n in parameters_names)
            {
                script.AppendLine($"f->AddVariable(\"{n}\");");
            }

            // The weight
            if (!string.IsNullOrWhiteSpace(weight_name))
            {
                if (_classificationType == ClassificationType.SignalBackground)
                {
                    script.AppendLine($"f->WeightExpression = \"{weight_name}\";");
                }
                else
                {
                    foreach (var c in _trainingSamples.Select(s => s._eventClass).Distinct())
                    {
                        script.AppendLine($"f->SetWeightExpression(\"{weight_name}\", \"{c}\");");
                    }
                }
            }

            // Now book all the methods that were requested
            foreach (var m in _methods)
            {
                m.Book(script, "f", parameters_names);
            }

            // Finally, do the training.
            script.AppendLine("f->TrainAllMethods();");

            // And do the evaluation, etc.
            script.AppendLine("f->TestAllMethods();");
            script.AppendLine("f->EvaluateAllMethods();");

            // Now, run the script!
            LocalBashHelpers.RunROOTInBash("training", script.ToString(), new DirectoryInfo(System.Environment.CurrentDirectory));

            // Write out the hash value
            using (var wr = hashFile.CreateText())
            {
                wr.WriteLine(hash);
            }
        }

        /// <summary>
        /// Run the training in-process
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="weight_name"></param>
        /// <param name="parameters_names"></param>
        /// <param name="trainingSamples"></param>
        /// <param name="hash"></param>
        /// <param name="outputFile"></param>
        /// <param name="hashFile"></param>
        /// <param name="resultObject"></param>
        /// <returns></returns>
        private void TrainInProcess(string jobName, string weight_name, List<string> parameters_names, Tuple<ROOTNET.Interface.NTTree, FileInfo, FileTrainingType, string>[] trainingSamples, int hash, FileInfo outputFile, FileInfo hashFile)
        {
            // This is the file where most of the basic results from the training will be written.
            var output = NTFile.Open(outputFile.FullName, "RECREATE");
            try
            {
                // Create the factory.
                var options = _tmva_options +
                    (_classificationType == ClassificationType.SignalBackground ? ":AnalysisType=Classification" : ":AnalysisType=Multiclass");
                var f = new ROOTNET.NTMVA.NFactory($"{jobName}-{hash}".AsTS(), output, options.AsTS());

                // Next, add the samples.
                bool isSimpleSigBack = _classificationType == ClassificationType.SignalBackground;
                foreach (var sample in trainingSamples)
                {
                    switch (sample.Item3)
                    {
                        case FileTrainingType.IsTraining:
                            if (isSimpleSigBack)
                            {
                                if (sample.Item4 == "Signal")
                                {
                                    f.AddSignalTree(sample.Item1, 1.0, ROOTNET.Interface.NTMVA.NTypes.ETreeType.kTraining);
                                }
                                else
                                {
                                    f.AddBackgroundTree(sample.Item1, 1.0, ROOTNET.Interface.NTMVA.NTypes.ETreeType.kTraining);
                                }
                            }
                            else
                            {
                                f.AddTree(sample.Item1, sample.Item4.AsTS(),
                                    1.0, new NTCut(""), ROOTNET.Interface.NTMVA.NTypes.ETreeType.kTraining);
                            }
                            break;
                        case FileTrainingType.IsBoth:
                            if (isSimpleSigBack)
                            {
                                if (sample.Item4 == "Signal")
                                {
                                    f.AddSignalTree(sample.Item1);
                                }
                                else
                                {
                                    f.AddBackgroundTree(sample.Item1);
                                }
                            }
                            else
                            {
                                f.AddTree(sample.Item1, sample.Item4.AsTS(), 1.0);
                            }
                            break;
                        case FileTrainingType.IsTesting:
                            if (isSimpleSigBack)
                            {
                                if (sample.Item4 == "Signal")
                                {
                                    f.AddSignalTree(sample.Item1, 1.0, ROOTNET.Interface.NTMVA.NTypes.ETreeType.kTesting);
                                }
                                else
                                {
                                    f.AddBackgroundTree(sample.Item1, 1.0, ROOTNET.Interface.NTMVA.NTypes.ETreeType.kTesting);
                                }
                            }
                            else
                            {
                                f.AddTree(sample.Item1, sample.Item4.AsTS(),
                                    1.0, new NTCut(""), ROOTNET.Interface.NTMVA.NTypes.ETreeType.kTesting);
                            }
                            break;
                        default:
                            throw new InvalidOperationException($"Inavlid program state: Do not know how to add samples of type {sample.Item3}.");
                    }
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
                    if (_classificationType == ClassificationType.SignalBackground)
                    {
                        f.WeightExpression = weight_name.AsTS();
                    }
                    else
                    {
                        foreach (var c in _trainingSamples.Select(s => s._eventClass).Distinct())
                        {
                            f.SetWeightExpression(weight_name.AsTS(), c.AsTS());
                        }
                    }
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
            }
            finally
            {
                output.Close();
            }
        }

        enum FileTrainingType
        {
            IsTraining,
            IsBoth,
            IsTesting
        }

        /// <summary>
        /// Extract the samples, splitting into signal and background as needed.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static IEnumerable<Tuple<ROOTNET.Interface.NTTree, FileInfo, FileTrainingType, string>> ExtractTrainingAndTestingSamples(SampleInfo s)
        {
            if (s._sample != null)
            {
                // If we aren't to split it at all, just go through "simply".
                if (s._isTrainingEvent == null)
                {
                    foreach (var v in s._sample.ToTTreeAndFile(s._title).Select(t => Tuple.Create(t.Item1, t.Item2, FileTrainingType.IsBoth, s._eventClass)))
                        yield return v;
                }
                else
                {
                    // We need to split it into signal and background
                    foreach (var v in s._sample.Where(s._isTrainingEvent).ToTTreeAndFile($"{s._title}-training").Select(t => Tuple.Create(t.Item1, t.Item2, FileTrainingType.IsTraining, s._eventClass)))
                    {
                        yield return v;
                    }
                    foreach (var v in s._sample.Where(qevt => !s._isTrainingEvent.Invoke(qevt)).ToTTreeAndFile($"{s._title}-testing").Select(t => Tuple.Create(t.Item1, t.Item2, FileTrainingType.IsTesting, s._eventClass)))
                    {
                        yield return v;
                    }
                }
            }
        }

        /// <summary>
        /// Get the list of parameter names to use from the object T's fields.
        /// </summary>
        /// <param name="parameters_names"></param>
        /// <returns></returns>
        /// <remarks>
        /// The weight is always pulled out as "Weight".
        /// </remarks>
        private Tuple<string, List<string>> GetParameterAndWeightNames()
        {
            var parameter_names = new List<string>();
            string weight_name = "";
            foreach (var field in typeof(T).GetFields().OrderBy(f => f.MetadataToken))
            {
                var name = field.Name;
                if (name == "Weight")
                {
                    weight_name = name;
                }
                else
                {
                    if (_use_variables.Count == 0 || (_use_variables.Contains(name)))
                    {
                        if (!_ignore_variables.Contains(name))
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
        /// <param name="isTrainingEvent">Return true if this is a training event, false otherwise</param>
        /// <returns></returns>
        public static Training<T> AsSignal<T>(this IQueryable<T> source, string title = "", Expression<Func<T,bool>> isTrainingEvent = null)
        {
            var t = new Training<T>();
            t.Signal(source, title, isTrainingEvent);
            return t;
        }

        /// <summary>
        /// Create a training environment, based on a source with a type and a partiular class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="eventClassName"></param>
        /// <param name="isTrainingEvent"></param>
        /// <returns></returns>
        public static Training<T> AsClass<T>(this IQueryable<T> source, string eventClassName = "", Expression<Func<T, bool>> isTrainingEvent = null, string title = null)
        {
            var t = new Training<T>();
            t.EventClass(source, eventClassName, isTrainingEvent, title);
            return t;
        }

        /// <summary>
        /// Create a training environment based on this background
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="isTrainingEvent">True if a particular event is a training event</param>
        /// <returns></returns>
        public static Training<T> AsBackground<T>(this IQueryable<T> source, string title = "", Expression<Func<T,bool>> isTrainingEvent = null)
        {
            var t = new Training<T>();
            t.Background(source, title, isTrainingEvent);
            return t;
        }
    }
}
