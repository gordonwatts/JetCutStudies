using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMVAUtilities
{
    /// <summary>
    /// Some utiltiies
    /// </summary>
    public static class FileInfoUtils
    {
        /// <summary>
        /// Build a filename reference that is short enough.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dir"></param>
        /// <param name="buildName"></param>
        /// <returns></returns>
        public static string ControlFilename(string name, DirectoryInfo dir, Func<string, string> buildName)
        {
            var outputTrainingRootInfo = Path.Combine(dir.FullName, buildName(name));
            var newName = name;
            while (outputTrainingRootInfo.Length >= 260)
            {
                var segments = newName.Split('.');
                var trimmed = segments
                    .Select(s => s.Length > 4 ? s.Substring(1) : s);
                var retrimmed = segments.Take(1).Concat(trimmed.Skip(1));
                newName = retrimmed.Aggregate((o, n) => $"{o}.{n}");
                outputTrainingRootInfo = Path.Combine(dir.FullName, buildName(newName));
            }
            return outputTrainingRootInfo;
        }

        /// <summary>
        /// Look for a marker file. If the file does not exist, call a clean up action first. Otherwise,
        /// skip that. Always call the execute action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="execute"></param>
        /// <param name="cleanupAction"></param>
        /// <param name="markerFile"></param>
        /// <returns></returns>
        public static T ActionIfMissingMarker<T>(this FileInfo markerFile, Func<T> execute, Action cleanupAction)
        {
            if (!markerFile.Exists)
            {
                cleanupAction();
            }

            // Run the function
            var started = DateTime.Now;
            T result;
            try
            {
                result = execute();
            }
            catch
            {
                // If the re-call after a successful marker file is present causes an exception, make
                // sure we will do the clean up next time through.
                if (markerFile.Exists)
                {
                    markerFile.Delete();
                }
                throw;
            }

            if (!markerFile.Exists)
            {
                using (var writer = markerFile.CreateText())
                {
                    writer.WriteLine($"Action started at {started.ToLongDateTimeString()}");
                    writer.WriteLine($"Action finished at {DateTime.Now.ToLongDateTimeString()}");
                }
                markerFile.Refresh();
            }

            return result;
        }

        /// <summary>
        /// Given a servies of CSV files, combine them.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static FileInfo CombineCSVFiles(this FileInfo[] files, FileInfo outputName = null, bool allHaveTitltes = true)
        {
            if (files.Where(f => f.Extension != ".csv").Any())
            {
                throw new ArgumentException("At least one file passed to us has a file type that isn't csv!");
            }

            if (files.Length == 0)
            {
                throw new ArgumentException("No files were passed to CombineCSVFiles");
            }

            // Generate the output file
            var outputFile = outputName == null
                ? new FileInfo($"{files[0].FullName}")
                : outputName;

            // Now open each file
            var linesToSkip = allHaveTitltes ? 1 : 0;

            using (var outputWriter = outputFile.CreateText())
            {
                // The first file is special if there are titles. No matter what we copy them all over.
                foreach (var line in files[0].AsLines())
                {
                    outputWriter.WriteLine(line);
                }

                // And for the rest of them.
                foreach (var file in files.Skip(1))
                {
                    foreach (var line in file.AsLines().Skip(linesToSkip))
                    {
                        outputWriter.WriteLine(line);
                    }
                }
                outputWriter.Close();
            }

            return outputFile;
        }

        /// <summary>
        /// Read the files as lines, one by one.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static IEnumerable<string> AsLines (this FileInfo f)
        {
            using (var reader = f.OpenText())
            {
                string line = "";
                while (line != null)
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        yield return line;
                    }
                }
            }
        }
    }
}
