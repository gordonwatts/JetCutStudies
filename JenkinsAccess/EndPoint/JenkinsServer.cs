using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static JenkinsAccess.Data.JenkinsDomain;

namespace JenkinsAccess.EndPoint
{
    class JenkinsServer
    {
        /// <summary>
        /// Info on an artifact
        /// </summary>
        public class Info
        {
            public string JobName;
            public string ArtifactName;
            public int BuildNumber;
        }

        /// <summary>
        /// Access to Jenkins REST API
        /// </summary>
        Lazy<WebClientAccess> _JenkinsEndPoint = new Lazy<WebClientAccess>(() => new WebClientAccess());

        Uri _artifactURI;

        string _jobName;
        string _buildName;
        string _artifactName;

        public JenkinsServer(Uri url)
        {
            _artifactURI = url;
            var segments = _artifactURI.Segments;

            // Get the job and artifact.
            var artifactInfo = segments.SkipWhile(s => s != "job/").Skip(1).Select(s => s.Trim('/')).ToArray();
            if (artifactInfo.Length != 4 && artifactInfo[2] == "artifact")
            {
                throw new ArgumentException($"The Jenkins artifact URI '{url}' is not in a format I recognize (.../jobname/build/artifact/artifact-name)");
            }
            _jobName = artifactInfo[0];
            _buildName = artifactInfo[1];
            _artifactName = artifactInfo[3];
        }

        /// <summary>
        /// Get everything setup. Some of this setup may require going up to the server.
        /// </summary>
        /// <returns></returns>
        private async Task Init()
        {
            // The only key here is if the build number is not determined at this point.
            if (_buildName == "lastSuccessfulBuild")
            {
                _buildName = (await GetLastSuccessfulBuild()).ToString();
            }
        }

        /// <summary>
        /// Fetch the last successful build.
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetLastSuccessfulBuild()
        {
            // Build the job URI, which will then return the JSON.
            var jobURIStem = GetJobURIStem();
            var r = await _JenkinsEndPoint.Value.FetchJSON<JenkinsJob>(jobURIStem);

            if (r.lastSuccessfulBuild == null)
            {
                throw new InvalidOperationException($"This Jenkins job does not yet have a successful build! {jobURIStem.OriginalString}");
            }

            return r.lastSuccessfulBuild.number;
        }

        /// <summary>
        /// Return a Uri of the job stem.
        /// </summary>
        /// <returns></returns>
        private Uri GetJobURIStem()
        {
            return new Uri(_artifactURI.OriginalString.Substring(0, _artifactURI.OriginalString.IndexOf(_jobName) + _jobName.Length));
        }

        /// <summary>
        /// Parse the URL to figure out everything we need
        /// </summary>
        /// <returns></returns>
        public async Task<Info> GetArtifactInfo()
        {
            await Init();
            return new Info()
            {
                JobName = _jobName,
                BuildNumber = int.Parse(_buildName),
                ArtifactName = _artifactName
            };
        }

        /// <summary>
        /// Download the artifact!
        /// </summary>
        /// <param name="artifactInfo"></param>
        /// <returns></returns>
        internal async Task Download(Info artifactInfo, FileInfo destination)
        {
            // Build the url
            var jobURI = GetJobURIStem();
            var artifactUri = new Uri($"{jobURI.OriginalString}/{artifactInfo.BuildNumber}/artifact/{artifactInfo.ArtifactName}");

            await _JenkinsEndPoint.Value.DownloadFile(artifactUri, destination);
        }
    }
}
