using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ParadoxDbCopier.IO;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace ParadoxDbCopier
{
    internal class Program
    {
        public static bool StartedFromGui { get; } = !Console.IsOutputRedirected
                                                     && !Console.IsInputRedirected
                                                     && !Console.IsErrorRedirected
                                                     && Environment.UserInteractive
                                                     && Environment.CurrentDirectory ==
                                                     Path.GetDirectoryName(Assembly
                                                         .GetEntryAssembly().Location)
                                                     && Console.CursorTop == 0 && Console.CursorLeft == 0
                                                     && Console.Title == Environment.GetCommandLineArgs()[0]
                                                     && Environment.GetCommandLineArgs()[0] ==
                                                     Assembly.GetEntryAssembly().Location;

        private static void Main(string[] args)
        {
            string inputFolder;
            string outputFolder;
            List<string> tableFilterList = null;

            bool useConfiguration = Convert.ToBoolean(ConfigurationManager.AppSettings["UseConfiguration"]);

            if (StartedFromGui || useConfiguration)
            {
                inputFolder = ConfigurationManager.AppSettings["InputFolder"];
                outputFolder = ConfigurationManager.AppSettings["OutputFolder"];

                if (string.IsNullOrWhiteSpace(inputFolder) || string.IsNullOrWhiteSpace(outputFolder))
                {
                    MessageBox.Show("Missing input or output folder in configuration", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TableNameList"]))
                {
                    tableFilterList = ConfigurationManager.AppSettings["TableNameList"]?.Split(',').ToList();
                }
            }
            else
            {
                if (args.Length < 2)
                {
                    Console.Error.WriteLine(
                        "Argument Error - Usage: ParadoxDbCopier [InputFolder] [OutputFolder] [Optional comma separated Table list]");
                    return;
                }

                inputFolder = args[0];
                outputFolder = args[1];
                tableFilterList = args.Length > 2 ? args[2].Split(',').ToList() : null;
            }

            var writerParameters = new DbWriterParameters
            {
                Scanner = new DbScanner(),
                OutputFolder = outputFolder,
                ColumnSeparator = ";",
                AddRefreshDateTimeColumn = true,
                OutputHeaderLine = true,
                TableFilterList = tableFilterList
            };

            var writer = new DbWriter(writerParameters);
            writer.WriteAll(inputFolder);
        }
    }
}