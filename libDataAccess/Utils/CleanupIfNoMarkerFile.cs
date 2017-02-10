using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    /// <summary>
    /// Write a marker file upon execution. If the marker file does not exist, execute some action
    /// </summary>
    public static class CleanupIfNoMarkerFile
    {
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
    }
}
