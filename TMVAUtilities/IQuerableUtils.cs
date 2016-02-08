﻿using LINQToTTreeLib.Files;
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
        public static NTTree ToTTree<T>(this IQueryable<T> source, string sampleTitle = "")
        {
            // Get the default directory. Look for a cxproj file, and if we don't find it
            // just use where we are now.
            var d = FindDirectoryWithFileMatching("*.csproj");
            if (d == null)
            {
                d = new DirectoryInfo(".");
            }
            var f = source.AsTTree(new FileInfo(Path.Combine(d.FullName, string.IsNullOrWhiteSpace(sampleTitle) ? $"{_f_index}.training.root" : $"{sampleTitle}.training.root")));
            _f_index++;
            var input = NTFile.Open(f.FullName, "READ");
            var tree = input.Get("mytree") as NTTree;
            _saver.Add(Tuple.Create(input, tree));
            return tree;
        }

        /// <summary>
        /// Make sure they don't get deleted while we are running!
        /// </summary>
        private static List<Tuple<ROOTNET.Interface.NTFile, NTTree>> _saver = new List<Tuple<ROOTNET.Interface.NTFile, NTTree>>();

        /// <summary>
        /// Find a directory that contains a pattern.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static DirectoryInfo FindDirectoryWithFileMatching(string v, DirectoryInfo startDir = null)
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
