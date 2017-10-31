using AtlasWorkFlows;
using AtlasWorkFlows.Jobs;
using AtlasWorkFlows.Panda;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace libDataAccess
{
    public static class GRIDJobs
    {
        /// <summary>
        /// Find the requested files that are output of a particular job.
        /// </summary>
        /// <param name="jobName">Name of the job</param>
        /// <param name="jobVersion">Version of the job that was run</param>
        /// <param name="sourceDataset">The source dataset</param>
        /// <param name="nFiles">How many files should we be running on?</param>
        /// <param name="intelligentLocal">If set to true, and we want only 1 or 2 files, and we can't get them first try, we will try to force a download</param>
        /// <returns></returns>
        public static FileInfo[] FindJobFiles(string jobName, int jobVersion, string sourceDataset, int nFiles = 0, Action<string> statusUpdate = null, bool intelligentLocal = false, int timeoutDownloadSecs = 3600*4)
        {
            string dataset = GetDatasetForJob(jobName, jobVersion, sourceDataset);
            var uris = DatasetManager.ListOfFilesInDataset(dataset, statusUpdate: m => statusUpdate($"{m} ({dataset})"));
            if (nFiles != 0)
            {
                uris = uris.OrderBy(u => u.Segments.Last()).Take(nFiles).ToArray();
            }

            Uri[] result = null;
            bool tryLocalIfFail = intelligentLocal && nFiles <= 2 && nFiles != 0;
            try
            {
                result = DatasetManager.MakeFilesLocal(uris, statusUpdate: m => statusUpdate($"{m} ({dataset})"));
            }
            catch (DatasetManager.NoLocalPlaceToCopyToException e)
            {
                if (statusUpdate != null)
                {
                    var naming = nFiles == 0 ? "all" : nFiles.ToString();
                    statusUpdate($"  -> Unable to MakeFilesLocal for {dataset} (for {naming} files): No local place to copy files: {e.Message}");
                }
            }

            if (result == null && tryLocalIfFail)
            {
                if (statusUpdate != null)
                {
                    statusUpdate($"  -> Unable to make {dataset} availible in any local repository.");
                    var naming = nFiles == 0 ? "all" : nFiles.ToString();
                    statusUpdate($"  -> Trying to fetch {dataset} to Local location ({naming} files)");
                }
                result = DatasetManager.CopyFilesTo(DatasetManager.FindLocation("Local"), uris, m => statusUpdate($"{m} ({dataset})"));
            }

            if (result == null)
            {
                throw new ArgumentException($"Unable to fetch dataset '{dataset}' - failing!");
            }

            return result.Select(furi => new FileInfo(furi.LocalPath)).ToArray();
        }

        /// <summary>
        /// Return the dataset name for a job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobVersion"></param>
        /// <param name="sourceDataset"></param>
        /// <returns></returns>
        private static string GetDatasetForJob(string jobName, int jobVersion, string sourceDataset)
        {
            // Find the job specification
            var job = JobParser.FindJob(jobName, jobVersion);
            if (job == null)
            {
                throw new ArgumentException($"Unable to find the definition of job {jobName} v{jobVersion}. Please create the jobspec files and re-run");
            }

            // Get the resulting job name for this guy.
            var pandaJobName = job.ResultingDatasetName(sourceDataset) + "/";

            string[] containers = GetContainersForPandJob(jobName, jobVersion, sourceDataset, pandaJobName);

            // Get the dataset, and then see if we can't access it. If we have been instructed,
            // try to make a local copy if it isn't here already.
            var dataset = containers.First();
            return dataset;
        }

        /// <summary>
        /// Fetch the Uri's of a set of data files
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobVersionNumber"></param>
        /// <param name="dsname"></param>
        /// <param name="nFiles"></param>
        /// <param name="statusUpdate"></param>
        /// <returns></returns>
        internal static Uri[] FindJobUris(string jobName, int jobVersionNumber, string dsname, int nFiles, Action<string> statusUpdate = null, string[] avoidPlaces = null)
        {
            // Get the files and the places where those files are located.
            var ds = GetDatasetForJob(jobName, jobVersionNumber, dsname);
            var allFiles = DatasetManager.ListOfFilesInDataset(ds).Take(nFiles == 0 ? 10000 : nFiles);
            var places = DatasetManager.ListOfPlacesHoldingAllFiles(allFiles, maxDataTier: 60)
                .Where(pl => avoidPlaces == null ? true : !avoidPlaces.Contains(pl))
                .ToArray();

            // If there is no place, then we are done
            if (places.Length == 0)
            {
                throw new FileNotFoundException($"Dataset {ds} was not found downloaded at any location I know about!");
            }

            // The places are sorted from best to worse. So start there.
            var p = places.First();

            // Now, get the uri's
            var uris = DatasetManager.LocalPathToFiles(p, allFiles);

            // We need to run these either locally or remotely.
            // This is a huristic, sadly. Lets hope!
            var newScheme = p.Contains("-linux") ? "remotebash" : "localwin";

            return uris
                .Select(u => new UriBuilder(u) { Scheme = newScheme }.Uri)
                .ToArray();
        }

        /// <summary>
        /// We cache the panda container lookup on the local machine so we can run offline in the future.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobVersion"></param>
        /// <param name="sourceDataset"></param>
        /// <param name="pandaJobName"></param>
        /// <returns></returns>
        private static string[] GetContainersForPandJob(string jobName, int jobVersion, string sourceDataset, string pandaJobName)
        {
            string[] containers = null;

            // Look in the local cache to see if the data is already there.
            var cacheLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AtlasSSH", "Task Container Cache");
            var cacheFile = new FileInfo(Path.Combine(cacheLocation, $"{pandaJobName}.txt"));
            if (cacheFile.Exists)
            {
                using (var r = cacheFile.OpenText())
                {
                    List<string> result = new List<string>();
                    while (!r.EndOfStream)
                    {
                        var l = r.ReadLine();
                        if (!string.IsNullOrWhiteSpace(l))
                        {
                            result.Add(l);
                        }
                    }
                    containers = result.ToArray();
                }
            }

            // If we don't have containers, fetch them (and then cache them so we don't have to again!).
            var pandaTaskStatus = "finished";
            if (containers == null)
            {
                // Now, to get the output dataset, we need to fetch the job.
                var pandaTask = pandaJobName.FindPandaJobWithTaskName(true);
                if (pandaTask == null)
                {
                    throw new ArgumentException($"No panda task found with name '{pandaJobName}' for job '{jobName}' v{jobVersion}. Perhaps it should be submitted with Invoke-GRIDJob {jobName} {jobVersion} {sourceDataset}?");
                }

                pandaTaskStatus = pandaTask.status;
                containers = pandaTask.DatasetNamesOUT();

                if (!cacheFile.Directory.Exists)
                {
                    cacheFile.Directory.Create();
                }
                using (var w = cacheFile.CreateText())
                {
                    foreach (var l in containers)
                    {
                        w.WriteLine(l);
                    }
                }
            }

            if (containers.Length > 1)
            {
                throw new ArgumentException($"There are more than one output container for the panda task {pandaJobName} - can't decide. Need code upgrade!! Thanks for the fish!");
            }
            if (containers.Length == 0)
            {
                if (pandaTaskStatus != "finished")
                {
                    throw new InvalidOperationException($"The panda task {pandaJobName} has not completed running yet! So no files to run on (status: {pandaTaskStatus}) for input file {sourceDataset} run by {jobName} v{jobVersion}!");
                }
                else
                {
                    throw new ArgumentException($"There are no output dataset containers for the panda task {pandaJobName} (for {sourceDataset} - {jobName} v{jobVersion}");
                }
            }

            return containers;
        }
    }
}
