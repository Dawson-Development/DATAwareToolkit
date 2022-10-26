using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DATAware_Tools.Classes
{
    public class MergeTool
    {
        MergeOptions _Options { get; set; }
        public String[] HeaderRecord { get; set; }
        public IEnumerable<FileParser> FileParsers;
        public bool FailedToImport = false;
        StringComparison checkCase = StringComparison.OrdinalIgnoreCase;
        public double currentProgress;

        public MergeTool(MergeOptions options)
        {
            _Options = options;
            if (_Options.strictCase) checkCase = StringComparison.Ordinal;
            SetupInputFiles().Wait();
        }

        private async Task SetupInputFiles()
        {
            if (_Options.verbose) Console.WriteLine("Setting up file instances");
            List<string> headers = new List<string>();
            List<FileParser> parsers = new List<FileParser>();

            //Create FileParser instance for each file
            try
            {
                foreach (string file in _Options.inputFiles)
                {
                    if (File.Exists(file))
                    {
                        if (_Options.verbose) Console.WriteLine("Extracing file properties from: " + file);
                        //Create parser object for the file
                        var parser = new FileParser(file, _Options.strictCase);

                        //Add headers to header list
                        foreach (string header in parser.HeaderRecord)
                        {
                            if (!headers.Any(h => h.Equals(header, checkCase)))
                            {
                                //Add da header foo!
                                headers.Add(header);
                            }
                        }
                        if (_Options.verbose) Console.WriteLine(
                            " -- Total Combined Headers: {0}\n -- Unique File Headers: {1}",
                            headers.Count(),
                            headers.Count() - parser.HeaderRecord.Count()
                        );
                        HeaderRecord = headers.ToArray();

                        //Add Parser to list
                        parsers.Add(parser);
                    }
                    else
                    {
                        Console.WriteLine("Unable to locate file: {0}", file);
                        Console.WriteLine("Terminating...");
                        Environment.Exit(4);
                    }
                }
                FileParsers = parsers;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Processing failed. Terminating application...");
                Console.Error.WriteLine(ex);
                Environment.Exit(4);
            }
        }

        public async Task Merge()
        {
            if (_Options.verbose) Console.WriteLine("Starting File Merge");
            FileWriter fp = new FileWriter(
                _Options.outputFile ?? //Arg defined output
                Path.GetDirectoryName(FileParsers.First().FullPath) + Configuration.FileNameConvention.MergeToolOutput //Default output
            );
            fp.Write(HeaderRecord); //Write the headers to disk

            var totalRows = FileParsers.Sum(t => t.RowCount);
            var loopCount = 0;

            foreach (FileParser parser in FileParsers)
            {
                string[] cRecord;
                while((cRecord = parser.Read()) != null)
                {
                    //Create new record from old one with new header mapping
                    List<string> record = new List<string>();

                    for(int i = 0; i < HeaderRecord.Count(); i++)
                    {
                        var index = Array.FindIndex(
                            parser.HeaderRecord,
                            t => t.Equals(HeaderRecord[i], checkCase) //t.IndexOf(HeaderRecord[i], checkCase) >= 0
                        );

                        if (index >= 0)
                        {
                             record.Add(cRecord[index]);
                        }else
                        {
                            record.Add("");
                        }
                    }
                    fp.Write(record.ToArray());

                    double progress;
                    if((progress = Math.Ceiling(((double)loopCount / (double)totalRows) * 100)) != currentProgress)
                    {
                        currentProgress = progress;
                            ConsoleHelper.WriteInline(string.Format("Current File: {0} | Progress {1}%",
                            parser.FileName,
                            progress
                        ));
                    }
                    loopCount++;
                }
                parser.Dispose();
            }
            fp.Dispose();
            ConsoleHelper.clearLine();
            Console.Write("Progress 100%");
            Console.WriteLine("\nDone merging files!");
        }
    }
}