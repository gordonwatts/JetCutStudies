using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    public class PathUtils
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
                newName = trimmed.Aggregate((o, n) => $"{o}.{n}");
                outputTrainingRootInfo = Path.Combine(dir.FullName, buildName(newName));
            }
            return outputTrainingRootInfo;
        }
    }
}
