using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ParadoxReader;

namespace ParadoxDbCopier.IO
{
    public class DbWriterParameters
    {
        public DbScanner Scanner { get; set; }
        public string OutputFolder { get; set; }
        public List<string> TableFilterList { get; set; }
        public string ColumnSeparator { get; set; }
        public bool OutputHeaderLine { get; set; }
        public bool AddRefreshDateTimeColumn { get; set; }
    }
    public class DbWriter
    {
        private readonly bool _addRefreshDateTimeColumn;
        private readonly string _columnSeparator;
        private readonly DbScanner _dbScanner;
        private readonly string _outputFolder;
        private readonly bool _outputHeaderLine;
        private readonly List<string> _tableFilterList;

        private bool _hasFailures;


        public DbWriter(DbWriterParameters parameters)
        {
            _outputFolder = parameters.OutputFolder;
            _tableFilterList = parameters.TableFilterList;
            _columnSeparator = parameters.ColumnSeparator;
            _dbScanner = parameters.Scanner;
            _outputHeaderLine = parameters.OutputHeaderLine;
            _addRefreshDateTimeColumn = parameters.AddRefreshDateTimeColumn;
        }

        /// <summary>
        /// Scans an input folder and iterates on each table file to export it into a output CSV, handles errors gracefully
        /// </summary>
        /// <param name="inputFolder"></param>
        public void WriteAll(string inputFolder)
        {
            var dataTables = _tableFilterList != null
                ? _dbScanner.GetDatabaseTables(inputFolder, _tableFilterList)
                : _dbScanner.GetDatabaseTables(inputFolder);

            if (!Directory.Exists(_outputFolder)) Directory.CreateDirectory(_outputFolder);

            Exception innerException = null;

            foreach (var table in dataTables)
                try
                {
                    WriteTable(table);
                }
                catch (Exception ex)
                {
                    _hasFailures = true;
                    innerException = ex;
                }

            if (_hasFailures) throw new Exception("Error copy paradox tables, at least one error.", innerException);
        }

        /// <summary>
        /// For a give DataTable this method output a CSV file with all its content
        /// </summary>
        /// <param name="table"></param>
        private void WriteTable(DataTable table)
        {
            var paradoxTable = new ParadoxTable(table.TableFolderPath, table.TableName);
            var outputFilePath = Path.Combine(_outputFolder, table.TableName + ".csv");
            var refreshDateTime = DateTime.UtcNow.ToString("u");

            var builder = new StringBuilder();

            if (_outputHeaderLine)
            {
                var headerLine = string.Join(_columnSeparator, paradoxTable.FieldNames);

                if (_addRefreshDateTimeColumn)
                    headerLine = string.Join(_columnSeparator, headerLine, "DataRefreshDateTime");

                builder.AppendLine(headerLine);
            }

            foreach (ParadoxRecord record in paradoxTable.Enumerate())
            {
                var outputLine = string.Join(_columnSeparator, record.DataValues.Select(Clean));

                if (_addRefreshDateTimeColumn)
                    outputLine = string.Join(_columnSeparator, outputLine, refreshDateTime);

                builder.AppendLine(outputLine);
            }

            File.WriteAllText(outputFilePath, builder.ToString());
        }

        private object Clean(object dataValue)
        {
            return dataValue is string s ? s.Replace(_columnSeparator, " ") : dataValue;
        }
    }
}