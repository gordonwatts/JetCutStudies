using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    public static class DirectoryUtils
    {
        public static IEnumerable<DirectoryInfo> AllParents (this DirectoryInfo dir)
        {
            while (dir.Parent != null)
            {
                yield return dir;
                dir = dir.Parent;
            }
        }
    }
}
