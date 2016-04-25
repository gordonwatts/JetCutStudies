using JenkinsAccess.EndPoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JenkinsAccess
{
    /// <summary>
    /// Provide "easy" access to artifacts from a Jenkins server.
    /// </summary>
    /// <remarks>
    /// Like everything in this library, this depends on windows generic credentials for any password access.
    /// </remarks>
    public class ArtifactAccess
    {
        /// <summary>
        /// Returns a downloaded artifact.
        /// </summary>
        /// <param name="artifactURI">The URI to the artifact (may contain a job number or a reference to latest)</param>
        /// <returns>A local reference to the artifact downloaded. It should not be modified!</returns>
        /// <remarks>
        /// If a reference is made to the "latestSuceessful" then the build server will be queried to find the latest successful
        /// job number. If the job number is for a file already downloaded, no download will occur. If the uri contains an explicit
        /// job number, and that file is already local for that job number, no web access will be made.
        /// 
        /// Files are stored in the user's temp directory. Deleting them at anytime is fine (as long as they aren't in direct use!!),
        /// as they will be automatically downloaded the next time a query is made.
        /// </remarks>
        static public async Task<FileInfo> GetArtifactFile(Uri artifactURI)
        {
            // Fetch access to the server
            var jenksInfo = new JenkinsServer(artifactURI);

            // Next, determine the job information for this guy
            var artifactInfo = await jenksInfo.GetArtifactInfo();

            // Build the file path where we will store it. If it is already there,
            // then we are done!
            var location = new FileInfo($"{Path.GetTempPath()}\\JenkinsArtifactCache\\{artifactInfo.JobName}\\{artifactInfo.BuildNumber}-{artifactInfo.ArtifactName}");
            if (location.Exists)
            {
                return location;
            }

            // If isn't there, then download it.
            await jenksInfo.Download(artifactInfo, location);
            location.Refresh();
            if (!location.Exists)
            {
                throw new InvalidOperationException($"Unable to download the Jenkins build artifact at the URL {artifactURI.OriginalString}.");
            }
            return location;
        }
    }
}
