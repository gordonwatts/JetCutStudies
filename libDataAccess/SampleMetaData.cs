using libDataAccess.Utils;
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
        /// Name of the sample.
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
        /// Initialize and populate sample metadata.
        /// </summary>
        /// <param name="filterEff"></param>
        /// <param name="eventsGenerated"></param>
        /// <param name="crossSection"></param>
        /// <param name="source"></param>
        public SampleMetaData(string name, double filterEff, int eventsGenerated, double crossSection, string source)
        {
            Name = name;
            FilterEfficiency = filterEff;
            EventsGenerated = eventsGenerated;
            CrossSection = crossSection;
            Source = source;
        }

        /// <summary>
        /// Load and parse the samples once.
        /// </summary>
        private static SampleMetaData[] _samples;

        /// <summary>
        /// Load up the metadata for a sample, pulling from our generic csv file.
        /// </summary>
        /// <remarks>
        /// The data in the CSV file was pulled from AMI, unless otherwise noted (see sheet).
        /// </remarks>
        /// <param name="sampleName"></param>
        public static SampleMetaData LoadFromCSV(string sampleName)
        {
            if (_samples == null)
            {
                var rootdir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
                var f = new FileInfo(Path.Combine(rootdir.FullName, "Sample Meta Data.csv"));
                if (!f.Exists)
                {
                    throw new FileNotFoundException($"Unable to load our sample metadata file from {f.FullName}.");
                }

                _samples = f
                    .ReadLines()
                    .Select(l => l.Split(","))
                    .Where(lst => lst.Length >= 3)
                    .Where(lst => lst[1].IsValidDouble() && lst[2].IsValidDouble() && lst[3].IsValidInt32())
                    .Select(lst => new SampleMetaData(lst[0], lst[1].ToDouble(), lst[3].ToInt32(), lst[2].ToDouble(), lst.Length >= 5 ? lst[4] : ""))
                    .ToArray();
            }

            // Find the sample. If we can't, fail pretty badly.
            return _samples
                .Where(sam => sam.Name == sampleName)
                .FirstOrDefault()
                .IfNull(_ => { throw new ArgumentException($"Unable to find sample meta data for sample '{sampleName}' in sample metadata file."); });
        }
    }
}
