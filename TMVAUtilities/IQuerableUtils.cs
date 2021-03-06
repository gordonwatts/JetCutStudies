﻿using LINQToTTreeLib.Files;
using ROOTNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Tuple;
using static TMVAUtilities.FileInfoUtilities;

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
        /// Convert a source to a TTree output file(s).
        /// </summary>
        /// <typeparam name="T">The object that we are to write out to the file as a tree. Needs to be something LINQToTTree understands.</typeparam>
        /// <param name="source">Sequence to write out</param>
        /// <param name="sampleTitle">The title to use with the TTree</param>
        /// <returns>Array of open TTree's and the files that contain them.</returns>
        public static Tuple<ROOTNET.Interface.NTTree, FileInfo>[] ToTTreeAndFile<T> (this IQueryable<T> source, string sampleTitle = "")
        {
            // Get the default directory. Look for a csproj file, and if we don't find it
            // just use where we are now.
            var d = FindDirectoryWithFileMatching("*.csproj");
            if (d == null)
            {
                d = new DirectoryInfo(".");
            }
            var fname = new FileInfo(Path.Combine(d.FullName, string.IsNullOrWhiteSpace(sampleTitle) ? $"{_f_index}.training.root" : $"{sampleTitle}.training.root"));
            var fmarker = new FileInfo(Path.Combine(d.FullName, string.IsNullOrWhiteSpace(sampleTitle) ? $"{_f_index}.training.root.marker" : $"{sampleTitle}.training.root.marker"));
            _f_index++;

            var f = fmarker
                .ActionIfMissingMarker(() => source.AsTTree(treeName: "TrainingTree", outputROOTFile: fname),
                                       () =>
                                       {
                                           foreach (var fToDelete in Directory.EnumerateFiles(fname.DirectoryName, $"{Path.GetFileNameWithoutExtension(fname.Name)}*.root"))
                                           {
                                               File.Delete(fToDelete);
                                           }
                                       }
                                       );

            // Convert into open files
            var results = (from finfo in f
                           let openFile = NTFile.Open(finfo.FullName, "READ")
                           select Create(openFile.Get("TrainingTree") as ROOTNET.Interface.NTTree, finfo, openFile))
                          .ToArray();

            // Make sure we hold onto them so they are garbage collected for some odd reason.
            _saver.AddRange(
                results
                .Select(i => Create(i.Item3, i.Item1 as NTTree))
                );

            return results
                .Select(i => Create(i.Item1, i.Item2))
                .ToArray();
        }

        /// <summary>
        /// Writes a querable out to a root file, and gets the tree.
        /// WARNING: Leaves a file open for reading.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static NTTree[] ToTTree<T>(this IQueryable<T> source, string sampleTitle = "")
        {
            // Get the default directory. Look for a cxproj file, and if we don't find it
            // just use where we are now.
            var d = FindDirectoryWithFileMatching("*.csproj");
            if (d == null)
            {
                d = new DirectoryInfo(".");
            }
            var f = source.AsTTree(treeName: "TrainingTree", outputROOTFile: new FileInfo(Path.Combine(d.FullName, string.IsNullOrWhiteSpace(sampleTitle) ? $"{_f_index}.training.root" : $"{sampleTitle}.training.root")));
            _f_index++;

            var results = (from finfo in f
                           let openFile = NTFile.Open(finfo.FullName, "READ")
                           let atree = openFile.Get("TrainingTree") as ROOTNET.NTTree
                           select Create(openFile, atree))
                          .ToArray();

            _saver.AddRange(results);

            return results
                .Select(i => i.Item2)
                .ToArray();
        }

        /// <summary>
        /// Make sure they don't get deleted while we are running!
        /// </summary>
        private static List<Tuple<ROOTNET.Interface.NTFile, NTTree>> _saver = new List<Tuple<ROOTNET.Interface.NTFile, NTTree>>();
    }
}
