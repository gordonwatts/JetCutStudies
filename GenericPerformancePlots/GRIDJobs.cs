using AtlasWorkFlows;
using AtlasWorkFlows.Jobs;
using AtlasWorkFlows.Panda;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericPerformancePlots
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
        /// <returns></returns>
        public static FileInfo[] FindJobFiles(string jobName, int jobVersion, string sourceDataset, int nFiles = 0, Action<string> statusUpdate = null)
        {
            // Find the job specification
            var job = JobParser.FindJob(jobName, jobVersion);
            if (job == null)
            {
                throw new ArgumentException($"Unable to find the definition of job {jobName} v{jobVersion}. Please create the jobspec files and re-run");
            }

            // Get the resulting job name for this guy.
            var pandaJobName = job.ResultingDatasetName(sourceDataset) + "/";

            // Now, to get the output dataset, we need to fetch the job.
            var pandaTask = pandaJobName.FindPandaJobWithTaskName(true);
            if (pandaTask == null)
            {
                throw new ArgumentException($"No panda task found with name '{pandaJobName}' for job '{jobName}' v{jobVersion}. Perhaps it should be submitted with Invoke-GRIDJob {jobName} {jobVersion} {sourceDataset}?");
            }

            var containers = pandaTask.DatasetNamesOUT();
            if (containers.Length > 1)
            {
                throw new ArgumentException($"There are more than one output container for the panda task {pandaTask.jeditaskid} - can't decide. Need code upgrade!! Thanks for the fish!");
            }
            if (containers.Length == 0)
            {
                if (pandaTask.status != "DONE")
                {
                    throw new InvalidOperationException($"The panda task {pandaTask.jeditaskid} has not completed running yet! So no files to run on (status: {pandaTask.status}) for input file {sourceDataset} run by {jobName} v{jobVersion}!");
                }
                else
                {
                    throw new ArgumentException($"There are no output dataset containers for the panda task {pandaTask.jeditaskid} (for {sourceDataset} - {jobName} v{jobVersion}");
                }
            }

            // Get the dataset, and then see if we can't access it.
            var dataset = containers.First();

            Func<string[], string[]> filter = nFiles == 0 ? (Func<string[], string[]>)null : flist => flist.OrderBy(f => f).Take(nFiles).ToArray();
            var r = GRIDDatasetLocator.FetchDatasetUris(dataset, statusUpdate, fileFilter: filter, timeoutDownloadSecs: 68*60*2);

            return r.Select(furi => new FileInfo(furi.LocalPath)).ToArray();
        }

    }
}
