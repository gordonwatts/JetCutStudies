using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMVAUtilities
{
    public static class FileInfoUtilities
    {
        /// <summary>
        /// Find a directory that contains a pattern.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static DirectoryInfo FindDirectoryWithFileMatching(string v, DirectoryInfo startDir = null)
        {
            var dir = startDir;
            if (dir == null)
            {
                dir = new DirectoryInfo(".");
            }

            while (dir != null)
            {
                if (dir.EnumerateFiles(v).Any())
                {
                    return dir;
                }
                dir = dir.Parent;
            }

            return null;
        }
    }
}
