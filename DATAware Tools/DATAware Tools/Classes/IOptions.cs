using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace DATAware_Tools.Classes
{
    [Verb("merge", HelpText = "Merge Files Together")]
    public class MergeOptions
    {
        [Option('i', "input", HelpText = "Data file(s) to process. Seperate by a single space")]
        public IEnumerable<string> inputFiles { get; set; }

        [Option("strictCase", Default = false, HelpText = "Strictly match the case values of headers.")]
        public bool strictCase { get; set; }

        [Option('o', "output", HelpText = "Output file")]
        public string outputFile { get; set; }

        [Option('v', "verbose", Default = false)]
        public bool verbose { get; set; }
    }

    [Verb("convert", HelpText = "Convert Data Format")]
    public class ConvertOptions
    {
        [Option('i', "input", HelpText = "Data file(s) to process. Seperate by a single space")]
        public string inputFile { get; set; }

        [Option("ignoreCase", HelpText = "Ignore the case of values")]
        public bool ignoreCase { get; set; }

        [Option('o', "output", HelpText = "Output file")]
        public string outputFile { get; set; }

        [Option("ext", Default = DataStructure.extensions.csv, HelpText = "Manually Define Data Type")]
        public DataStructure.extensions extension { get; set; }
    }

    [Verb("info", HelpText= "Get basic information about files")]
    public class FileInfoOptions
    {
        [Option('i', "input", Required = true)]
        public string InputFile { get; set; }

        [Option('c', "countlines", HelpText = "Count total number of lines of a file")]
        public bool CountLines { get; set; }

    }
}
