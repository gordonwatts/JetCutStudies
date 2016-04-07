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

            // Copy over the output training root file.
            var outputTrainingRootInfo = Path.Combine(dir.FullName, $"{name}.training.root");
            TrainingOutputFile.CopyTo(outputTrainingRootInfo, true);

            // Next, each of the weight files
            foreach (var m in MethodList)
            {
                var originalWeightFile = m.WeightFile;
                var finalName = Path.Combine(dir.FullName, $"{name}_{m.Name}.weights.xml");
                originalWeightFile.CopyTo(finalName, true);
            }
        }
    }
}
