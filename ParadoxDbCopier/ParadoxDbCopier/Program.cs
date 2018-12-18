using System;
using System.Collections.Generic;
using System.Linq;
using ParadoxDbCopier.IO;

namespace ParadoxDbCopier
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine(
                    "Argument Error - Usage: ParadoxDbCopier [InputFolder] [OutputFolder] [Optional comma separated Table list]");
                return;
            }

            var inputFolder = args[0];
            var outputFolder = args[1];

            var tableFilterList = args.Length > 2 ? args[2].Split(',').ToList() : null;

            var scanner = new DbScanner();
            var writer = new DbWriter(scanner, outputFolder, tableFilterList, columnSeparator: ";",
                outputHeaderLine: true,
                addRefreshDateTimeColumn: true);

            writer.WriteAll(inputFolder);
        }
    }
}