using libDataAccess.Utils;
using System.IO;

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
            var outputTrainingRootInfo = PathUtils.ControlFilename(name, dir, n => $"{n}.training.root");
            TrainingOutputFile.CopyTo(outputTrainingRootInfo, true);

            // Next, each of the weight files
            foreach (var m in MethodList)
            {
                var originalWeightFile = m.WeightFile;
                var finalName = PathUtils.ControlFilename (name, dir, n => $"{n}_{m.Name}.weights.xml");
                originalWeightFile.CopyTo(finalName, true);
            }
        }
    }
}
