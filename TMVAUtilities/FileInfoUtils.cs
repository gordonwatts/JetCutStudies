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
