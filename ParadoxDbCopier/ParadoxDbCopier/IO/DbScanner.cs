using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ParadoxDbCopier.IO
{
    internal class DataTable
    {
        public string TableFolderPath { get; set; }
        public string TableName { get; set; }
    }

    internal class DbScanner
    {
        /// <summary>
        /// Scans for data tables within a folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public IEnumerable<DataTable> GetDatabaseTables(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.db").Select(filePath =>
                new DataTable
                {
                    TableName = Path.GetFileNameWithoutExtension(filePath),
                    TableFolderPath = folderPath
                });
        }

        /// <summary>
        /// Converts table names into DataTables
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="dataTableNames"></param>
        /// <returns></returns>
        public IEnumerable<DataTable> GetDatabaseTables(string folderPath, IEnumerable<string> dataTableNames)
        {
            var resultDataTables = new List<DataTable>();

            foreach (var tableName in dataTableNames)
                if (File.Exists(Path.Combine(folderPath, tableName + ".db")))
                    resultDataTables.Add(new DataTable
                    {
                        TableName = tableName,
                        TableFolderPath = folderPath
                    });

            return resultDataTables;
        }
    }
}