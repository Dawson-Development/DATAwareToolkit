using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATAware_Tools.Classes;
using CommandLine;
using System.Threading;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using Newtonsoft.Json;
using NamedPipeWrapper;
using System.Runtime.InteropServices;

namespace DATAware_Tools
{
    class Program
    {
        static Mutex mutexInstance = new Mutex(true, "DATAware_Tools");
        public static List<string> dataFiles = new List<string>();

        static void Main(string[] args)
        {
            //args = new string[] {
            //    "merge",
            //        "-i",
            //            @"Y:\Taxes\Georgia\Rabun\PPR\2019\2019 PPR\Data\Final Data\Darwin\Aircraft_Adj.csv",
            //            @"Y:\Taxes\Georgia\Rabun\PPR\2019\2019 PPR\Data\Final Data\Darwin\Boats_Adj.csv",
            //            @"Y:\Taxes\Georgia\Rabun\PPR\2019\2019 PPR\Data\Final Data\Darwin\PersonalBusiness_Adj.csv",
            //        "-v"
            //};

            //args = new string[]
            //{
            //    "info",
            //        "-c",
            //        "-i",
            //        @"E:\Git\Tools and Utilities\DATAware Tools\DATAware Tools\DATAware Tools\bin\Debug\Tools\Merge.ico"
            //};

            Parser.Default.ParseArguments<FileInfoOptions, MergeOptions, ConvertOptions>(args)
                .MapResult(
                    (FileInfoOptions opts) => FileInfo(opts),
                    (MergeOptions opts) => MergeFiles(opts),
                    (ConvertOptions opts) => 0,
                    errs => { Console.ReadLine(); return 1; } 
            );
        }       

        static int FileInfo(FileInfoOptions options)
        {
            if (options.CountLines)
            {
                Func<int> countLines = () =>
                {
                    try
                    {
                        Console.WriteLine("Total Lines: " + File.ReadLines(options.InputFile).Count());
                        return 0;
                    }
                    catch
                    {
                        Console.WriteLine("Cant parse file...");
                        Console.WriteLine("Make sure its not open and in a supported format.");
                        return 1;
                    }
                };

                switch (Path.GetExtension(options.InputFile).ToLower())
                {
                    case ".csv":
                    case ".txt":
                        countLines();
                        break;
                    case ".xls":
                    case ".xlsx":
                        Console.WriteLine("Format Not Supported Yet");
                        break;
                    default:
                        Console.WriteLine("Unsupported data format.");
                        Console.WriteLine("Attempting to read file as ascii...");
                        countLines();
                        break;
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return 0;
        }

        /// <summary>
        /// Merge Tool
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        static int MergeFiles(MergeOptions options)
        {
            if (mutexInstance.WaitOne(TimeSpan.FromSeconds(2), true)) //Server 
            {
                var server = new NamedPipeServer<string>("DATAware_Tools");
                server.ClientMessage += delegate (NamedPipeConnection<string, string> conn, string message)
                {
                    conn.PushMessage(Configuration.NamedPipe.MessageReceived);
                    dataFiles.AddRange(JsonConvert.DeserializeObject<string[]>(message));
                };
                server.Start();

                var startTime = DateTime.UtcNow;
                while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(5))
                {
                    //Waiting for external input
                }

                dataFiles.AddRange(options.inputFiles); //Add server data files to input files with client
                options.inputFiles = dataFiles.ToArray(); //Set inputFiles to data files
                MergeTool mt = new MergeTool(options);
                if (!mt.FailedToImport) //Import files and check for failure
                {
                    mt.Merge().Wait(); //Merge the files!!!

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return 0;
                }
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return 1; //Failure
            }
            else //Client
            {
                Console.WriteLine("Sending to parent thread...");
                var client = new NamedPipeClient<string>("DATAware_Tools");

                client.ServerMessage += delegate (NamedPipeConnection<string, string> conn, string message)
                {
                    if (message.Equals(Configuration.NamedPipe.MessageReceived))
                    {
                        client.Stop();
                        client.WaitForDisconnection();
                        Environment.Exit(0);
                    }
                };

                client.Start();
                client.WaitForConnection();
                client.PushMessage(JsonConvert.SerializeObject(options.inputFiles));

                var startTime = DateTime.UtcNow;
                while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(5))
                {
                    //Wait for response from server
                }
                return 1; //Failure
            }
        }

        static int ConvertFiles(ConvertOptions options)
        {
            ConvertTool convertTool = new ConvertTool(options);

            switch (Path.GetExtension(options.inputFile).ToLower())
            {
                //case ".csv":
                //case ".txt":
                //    countLines();
                //    break;
                case ".xls":

                case ".xlsx":
                    Console.WriteLine("Format Not Supported Yet");
                    break;
                default:
                    Console.WriteLine("Unsupported data format.");
                    break;
            }
            return 0;
        }
    }
}
