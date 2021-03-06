﻿using libDataAccess.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess
{
    public class SampleMetaData
    {
        /// <summary>
        /// Full rucio dataset name of the sample.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The generator level filter efficiency
        /// </summary>
        public double FilterEfficiency { get; private set; }

        /// <summary>
        /// Number of events that were generated
        /// </summary>
        public int EventsGenerated { get; private set; }

        /// <summary>
        /// Get the cross section in nb-1.
        /// </summary>
        public double CrossSection { get; private set; }

        /// <summary>
        /// Location where the data was sourced from (e.g. AMI, Rachel, etc.).
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// The short name that we can use to refer to this everywhere else.
        /// </summary>
        public string NickName { get; private set; }

        /// <summary>
        /// Tags associated with this sample
        /// </summary>
        public string[] Tags { get; private set; }

        /// <summary>
        /// Returns true if tag returns having a name.
        /// </summary>
        /// <param name="tname"></param>
        /// <returns></returns>
        public bool HasTag(string tname)
        {
            return Tags.Contains(tname);
        }

        /// <summary>
        /// Initialize and populate sample metadata.
        /// </summary>
        /// <param name="filterEff"></param>
        /// <param name="eventsGenerated"></param>
        /// <param name="crossSection"></param>
        /// <param name="source"></param>
        public SampleMetaData(string name, double filterEff, int eventsGenerated, double crossSection, string source, string nickname, string[] tags)
        {
            Name = name;
            FilterEfficiency = filterEff;
            EventsGenerated = eventsGenerated;
            CrossSection = crossSection;
            Source = source;
            NickName = nickname;
            Tags = tags;
        }

        /// <summary>
        /// Load and parse the samples once.
        /// </summary>
        private static SampleMetaData[] _samples;


        [Serializable]
        public class SampleNotFoundInListException : Exception
        {
            public SampleNotFoundInListException() { }
            public SampleNotFoundInListException(string message) : base(message) { }
            public SampleNotFoundInListException(string message, Exception inner) : base(message, inner) { }
            protected SampleNotFoundInListException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        /// <summary>
        /// Load up the metadata for a sample, pulling from our generic csv file.
        /// </summary>
        /// <remarks>
        /// The data in the CSV file was pulled from AMI, unless otherwise noted (see sheet).
        /// </remarks>
        /// <param name="sampleName">The full dataset name or the nick name of the sample we are looking for.</param>
        /// <param name="tagnames">List of tags the samples must also have</param>
        public static SampleMetaData LoadFromCSV(string sampleName, params string[] tagnames)
        {
            LoadMetaData();

            // Find the sample. If we can't, fail pretty badly.
            return _samples
                .Where(sam => sam.Name == sampleName || sam.NickName == sampleName)
                .Where(sample => tagnames.All(tn => sample.HasTag(tn)))
                .FirstOrDefault()
                .IfNull(_ => { throw new SampleNotFoundInListException($"Unable to find sample meta data for sample '{sampleName}' in sample metadata file."); });
        }

        /// <summary>
        /// Load up all meta data.
        /// </summary>
        private static void LoadMetaData()
        {
            if (_samples == null)
            {
                var directories = new[] { new FileInfo(Assembly.GetExecutingAssembly().Location).Directory, new DirectoryInfo(".") };
                var f = directories
                    .SelectMany(d => d.AllParents())
                    .Select(d => new FileInfo(Path.Combine(d.FullName, "Sample Meta Data.csv")))
                    .Where(mf => mf.Exists)
                    .FirstOrDefault();
                if (f == null || !f.Exists)
                {
                    throw new FileNotFoundException($"Unable to load our sample metadata file from 'Sample Meta Data.csv'.");
                }

                _samples = f
                    .ReadLines()
                    .Select(l => l.Split(","))
                    .Where(lst => lst.Length >= 3)
                    .Where(lst => lst[1].IsValidDouble() && lst[2].IsValidDouble() && lst[3].IsValidInt32())
                    .Select(lst => new SampleMetaData(lst[0].Trim(), lst[1].ToDouble(), lst[3].ToInt32(), lst[2].ToDouble(), lst.Length >= 5 ? lst[4] : "", lst[5].Trim(), lst[6].Trim().Split('+')))
                    .ToArray();
            }
        }

        /// <summary>
        /// Return any samples with a particular set of tags, including the null list if no matches are found.
        /// All samples returned will have all tags given.
        /// </summary>
        /// <param name="tagnames">List of all tag names that we are going to require</param>
        /// <returns></returns>
        public static IEnumerable<SampleMetaData> AllSamplesWithTag(params string[] tagnames)
        {
            LoadMetaData();
            return _samples
                .Where(s => tagnames.Where(tn => !string.IsNullOrWhiteSpace(tn)).All(tn => s.HasTag(tn)));
        }
    }
}
