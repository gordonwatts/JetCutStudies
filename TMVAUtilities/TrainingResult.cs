using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMVAUtilities
{
    /// <summary>
    /// Hold onto the result of a TMVA training.
    /// </summary>
    /// <typeparam name="TR"></typeparam>
    public class TrainingResult<TR>
    {
        public DirectoryInfo OutputName;

        public FileInfo TrainingOutputFile { get; set; }

        public string JobName { get; internal set; }

        public Method<TR>[] MethodList { get; internal set; }

        /// <summary>
        /// Copy the output .root file and xml file to a common name, rather than the one with the crazy
        /// names we are currently using.
        /// </summary>
        public void CopyToJobName(string name = "JetMVATraining", DirectoryInfo dir = null)
        {
            dir = dir == null ? new DirectoryInfo(".") : dir;

            // Copy over the training output root file.
            var outputTrainingRootInfo = ControlFilename(name, dir, n => $"{n}.training.root");
            TrainingOutputFile.CopyTo(outputTrainingRootInfo, true);

            // Next, each of the weight files
            foreach (var m in MethodList)
            {
                var originalWeightFile = m.WeightFile;
                var finalName = ControlFilename (name, dir, n => $"{n}_{m.Name}.weights.xml");
                originalWeightFile.CopyTo(finalName, true);
            }
        }

        /// <summary>
        /// Build a filename reference that is short enough.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dir"></param>
        /// <param name="buildName"></param>
        /// <returns></returns>
        private static string ControlFilename(string name, DirectoryInfo dir, Func<string, string> buildName)
        {
            var outputTrainingRootInfo = Path.Combine(dir.FullName, buildName(name));
            var newName = name;
            while (outputTrainingRootInfo.Length >= 260)
            {
                var segments = newName.Split('.');
                var trimmed = segments
                    .Select(s => s.Length > 4 ? s.Substring(1) : s);
                newName = trimmed.Aggregate((o, n) => $"{o}.{n}");
                outputTrainingRootInfo = Path.Combine(dir.FullName, buildName(newName));
            }
            return outputTrainingRootInfo;
        }
    }
}
