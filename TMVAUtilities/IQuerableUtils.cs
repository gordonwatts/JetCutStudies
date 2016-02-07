using LINQToTTreeLib.Files;
using ROOTNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMVAUtilities
{
    /// <summary>
    /// Some helpers to make code in other places look "nice"
    /// </summary>
    static class IQuerableUtils
    {
        /// <summary>
        /// Keep track of how many files we've written.
        /// </summary>
        private static int _f_index = 0;

        /// <summary>
        /// Writes a querable out to a root file, and gets the tree.
        /// WARNING: Leaves a file open for reading.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static NTTree ToTTree<T>(this IQueryable<T> source)
        {
            var f = source.AsTTree(new FileInfo($"{_f_index}.training.root"));
            var input = NTFile.Open(f.FullName, "READ");
            var tree = input.Get("mytree") as NTTree;
            return tree;
        }
    }
}
