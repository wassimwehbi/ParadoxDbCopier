using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ParadoxReader;

namespace ParadoxDbCopier.IO
{
    internal class DbWriter
    {
        private readonly bool _addRefreshDateTimeColumn;
        private readonly string _columnSeparator;
        private readonly DbScanner _dbScanner;
        private readonly string _outputFolder;
        private readonly bool _outputHeaderLine;
        private readonly List<string> _tableFilterList;

        private bool _hasFailures;


        internal DbWriter(DbScanner scanner, string outputFolder, List<string> tableFilterList, string columnSeparator,
            bool outputHeaderLine = true,
            bool addRefreshDateTimeColumn = false)
        {
            _outputFolder = outputFolder;
            _tableFilterList = tableFilterList;
            _columnSeparator = columnSeparator;
            _dbScanner = scanner;
            _outputHeaderLine = outputHeaderLine;
            _addRefreshDateTimeColumn = addRefreshDateTimeColumn;
        }

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
                var outputLine = string.Join(_columnSeparator, record.DataValues);

                if (_addRefreshDateTimeColumn)
                    outputLine = string.Join(_columnSeparator, outputLine, refreshDateTime);

                builder.AppendLine(outputLine);
            }

            File.WriteAllText(outputFilePath, builder.ToString());
        }
    }
}