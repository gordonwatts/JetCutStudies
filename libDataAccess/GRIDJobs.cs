using AtlasWorkFlows;
using AtlasWorkFlows.Jobs;
using AtlasWorkFlows.Panda;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        public static async Task<FileInfo[]> FindJobFiles(string jobName, int jobVersion, string sourceDataset, int nFiles = 0, Action<string> statusUpdate = null, bool intelligentLocal = false, int timeoutDownloadSecs = 3600*4)
        {
            string dataset = GetDatasetForJob(jobName, jobVersion, sourceDataset);
            var uris = await DataSetManager.ListOfFilesInDataSetAsync(dataset, statusUpdate: m => statusUpdate($"{m} ({dataset})"));
            if (nFiles != 0)
            {
                uris = uris.OrderBy(u => u.Segments.Last()).Take(nFiles).ToArray();
            }

            Uri[] result = null;
            bool tryLocalIfFail = intelligentLocal && nFiles <= 2 && nFiles != 0;
            try
            {
                result = await DataSetManager.MakeFilesLocalAsync(uris, statusUpdate: m => statusUpdate($"{m} ({dataset})"));
            }
            catch (NoLocalPlaceToCopyToException e)
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
                result = await DataSetManager.CopyFilesToAsync(await DataSetManager.FindLocation("Local"), uris, m => statusUpdate($"{m} ({dataset})"));
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
            var pandaJobName = job.ResultingDataSetName(sourceDataset) + "/";

            string[] containers = GetContainersForPandJob(jobName, jobVersion, sourceDataset, pandaJobName);

            // Get the dataset, and then see if we can't access it. If we have been instructed,
            // try to make a local copy if it isn't here already.
            var dataset = containers.First();
            return dataset;
        }

        /// <summary>
        /// Random number generator that will send stuff to different machines.
        /// </summary>
        private static Lazy<Random> _random = new Lazy<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// Fetch the Uri's of a set of data files
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobVersionNumber"></param>
        /// <param name="dsname"></param>
        /// <param name="nFiles"></param>
        /// <param name="statusUpdate"></param>
        /// <returns></returns>
        internal static async Task<Uri[]> FindJobUris(string jobName, int jobVersionNumber, string dsname, int nFiles,
            Action<string> statusUpdate = null,
            string[] avoidPlaces = null,
            string[] preferPlaces = null)
        {
            // Get the files and the places where those files are located.
            var ds = GetDatasetForJob(jobName, jobVersionNumber, dsname);
            var allFiles = (await DataSetManager.ListOfFilesInDataSetAsync(ds)).Take(nFiles == 0 ? 10000 : nFiles);
            var places = (await DataSetManager.ListOfPlacesHoldingAllFilesAsync(allFiles, maxDataTier: 60))
                .Where(pl => avoidPlaces == null ? true : !avoidPlaces.Contains(pl))
                .ToArray();

            // If anything is prefered, take it.
            if (preferPlaces != null)
            {
                var prefered = places.Where(plac => preferPlaces.Contains(plac)).ToArray();
                if (prefered.Length > 0)
                {
                    places = prefered;
                }
            }

            // If there is no place, then we are done
            if (places.Length == 0)
            {
                throw new FileNotFoundException($"Dataset {ds} was not found downloaded at any location I know about!");
            }

            // The places are sorted from best to worse. So start there.
            var p = places.First();

            // Now, get the uri's
            var uris = await DataSetManager.LocalPathToFilesAsync(p, allFiles);

            // We need to run these either locally or remotely.
            // This is a huristic, sadly. Lets hope!
            var newScheme = p.Contains("-linux") ? "remotebash" : "localwin";

            return uris
                .Select(u => u.UpdateUriWithMachineAccessAndScheme(newScheme))
                .ToArray();
        }

        private struct ClusterMachineInfo
        {
            public string User;
            public int nConnections;
            public string[] MachineList;

            /// <summary>
            /// Return a random machine from our collection of machines.
            /// </summary>
            /// <returns></returns>
            public string RandomMachine() => MachineList[_random.Value.Next(0, MachineList.Length)];
        }

        /// <summary>
        /// Cache the cluster info we read from an externa file.
        /// </summary>
        private static Dictionary<string, ClusterMachineInfo> _cluster_machine_info = new Dictionary<string, ClusterMachineInfo>();

        /// <summary>
        /// Update a Uri:
        ///  - New scheme
        ///  - If we can find it, with randomized machine information
        /// </summary>
        /// <param name="u"></param>
        /// <param name="newScheme"></param>
        /// <returns></returns>
        private static Uri UpdateUriWithMachineAccessAndScheme (this Uri u, string newScheme)
        {
            // Look at the current machine name and see if we have any sort of a cluster information file.
            var mName = u.Host;
            return _cluster_machine_info.ContainsKey(mName) ? u.UpdateUriFromCache(newScheme)
                : File.Exists(ClusterFilename(mName)) ? u.UpdateUriFromClusterFile(newScheme)
                : new UriBuilder(u) { Scheme = newScheme}.Uri;
        }

        /// <summary>
        /// Get back a Uri for a machine we have already cached info for.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="newSchemeName"></param>
        /// <returns></returns>
        private static Uri UpdateUriFromCache (this Uri u, string newSchemeName)
        {
            var info = _cluster_machine_info[u.Host];
            return new UriBuilder(u) { Scheme = newSchemeName, Query = $"connections={info.nConnections}", Host=info.RandomMachine(), UserName = info.User }.Uri;
        }

        /// <summary>
        /// Load up some cluster info from a file, and then update the Uri.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="newSchemeName"></param>
        /// <returns></returns>
        private static Uri UpdateUriFromClusterFile(this Uri u, string newSchemeName)
        {
            // Load the cache
            var machine_info = File.ReadAllLines(ClusterFilename(u.Host))
                .Where(l => !l.StartsWith("#"))
                .Select(l => l.Split(new[] { '=' }, 2))
                .Where(k => k.Length == 2)
                .ToDictionary(k => k[0].Trim(), k => k[1].Trim());

            var info = new ClusterMachineInfo()
            {
                MachineList = machine_info["machines"].Split(',').Select(m => m.Trim()).ToArray(),
                User = machine_info["user"],
                nConnections = Int32.Parse(machine_info["connections"])
            };
            _cluster_machine_info[u.Host] = info;

            // Do the substitution
            return u.UpdateUriFromCache(newSchemeName);
        }

        /// <summary>
        /// Return the cluster filename file for a particular machine name.
        /// </summary>
        /// <param name="mName"></param>
        /// <returns></returns>
        private static string ClusterFilename(string mName)
        {
            return $"{mName}.cluster_machines";
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
                containers = pandaTask.DataSetNamesOUT();

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
