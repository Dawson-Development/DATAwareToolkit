using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;

namespace DATAware_Tools.Classes
{
    public class FileParser
    {
        StreamReader sReader { get; set; }
        CsvReader cReader { get; set; }
        StreamWriter sWriter { get; set; }
        CsvWriter cWriter { get; set; }
        public string[] HeaderRecord { get; set; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public int RowCount { get; set; }
        StringComparer _checkCase { get; set; }

        public FileParser(string fileLocation, bool checkCase)
        {
            FullPath = fileLocation;
            FileName = Path.GetFileNameWithoutExtension(fileLocation);
            _checkCase = (checkCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

            RowCount = File.ReadLines(fileLocation).Count() -1;
            OpenReader(); //Open lock on reader
            Getheaders();
        }

        private void Getheaders()
        {
            if (HeaderRecord == null)
            {
                //Get the header record
                cReader.Read();
                cReader.ReadHeader();

                List<string> dataHeader = cReader.Context.HeaderRecord.ToList();
                var counts = dataHeader.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).ToDictionary(x => x.Key, x => x.Count());
                var values = counts.ToDictionary(x => x.Key, x => 0);
                for (int i=0;i<dataHeader.Count();i++)
                {
                    if (counts.ContainsKey(dataHeader[i]))
                    {
                        var key = dataHeader[i];
                        if(values[key] > 0)
                        {
                            dataHeader[i] += "_dup" + (values[key] > 1 ? values[key].ToString() : "");
                        }
                        values[key]++;
                    }
                }
                HeaderRecord = dataHeader.ToArray();
            }
        }

        public string[] Read()
        {
            OpenReader();
            cReader.Read();
            return cReader.Context.Record;
        }

        private bool OpenReader()
        {
            if(cReader == null)
            {
                try
                {
                    sReader = new StreamReader(FullPath);
                    cReader = new CsvReader(sReader);
                    return true;
                }
                catch(Exception ex) //Cant open file
                {
                    Console.WriteLine("Unable to open file: " + FullPath);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Cancelling Tool");
                    Environment.Exit(4);
                    return false;
                }
            }
            return true;
        }
        
        public void Dispose()
        {
            sReader.Dispose();
            cReader.Dispose();
        }
    }

    public class FileWriter
    {
        StreamWriter sWriter { get; set; }
        CsvWriter cWriter { get; set; }
        public string FullPath { get; set; }

        public FileWriter(string fileLocation)
        {
            FullPath = fileLocation;
        }

        public void Write(string[] row)
        {
            OpenWriter();
            foreach(string field in row)
            {
                cWriter.WriteField(field);
            }
            cWriter.NextRecord();
        }

        private bool OpenWriter()
        {
            if (cWriter == null)
            {
                try
                {
                    sWriter = new StreamWriter(FullPath);
                    cWriter = new CsvWriter(sWriter);
                    cWriter.Configuration.QuoteAllFields = true;
                    return true;
                }
                catch (Exception ex) //Cant open file
                {
                    Console.WriteLine("Unable to write file: " + FullPath);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Cancelling Tool");
                    Console.ReadLine();
                    Environment.Exit(4);
                    return false;
                }
            }
            return true;
        }

        public void Dispose()
        {
            sWriter.Dispose();
            cWriter.Dispose();
        }
    }
}
