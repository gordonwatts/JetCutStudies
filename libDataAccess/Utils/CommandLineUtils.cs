using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libDataAccess.Utils
{
    /// <summary>
    /// Handle the basic (common) command line options.
    /// </summary>
    public class CommandLineUtils
    {
        public class Options
        {
            [Option("UseFullDataset", Default = false, HelpText = "Use full dataset rather than test dataset size.")]
            public bool UseFullDataset { get; set; }
        }

        /// <summary>
        /// Parse the command line arguments, and deal with their execution.
        /// </summary>
        /// <param name="args"></param>
        public static void Parse(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);

            result.MapResult(
                options => {
                    if (options.UseFullDataset)
                    {
                        Files.NFiles = 0;
                    } else
                    {
                        Files.NFiles = 1;
                    }
                    return 0;
                },
                errors => {
                    foreach (var err in errors)
                    {
                        Console.WriteLine($"Error parsing command line: {err.ToString()}");
                    }
                    return 1;
                });
        }
    }
}
