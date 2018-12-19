using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ParadoxDbCopier.IO;
using ParadoxDbCopierUnitTests.data;
using ParadoxReader;
using Xunit;

namespace ParadoxDbCopierUnitTests
{
    public class UnitTests
    {
        private void AssertAreEqual(List<DataTable> expectedDataTables, List<DataTable> actualDataTables)
        {
            Assert.Equal(expectedDataTables.Count, actualDataTables.Count);

            foreach (var table in expectedDataTables) Assert.Contains(table, actualDataTables);
        }

        private DirectoryInfo CreateTestDirectoryWithTables(out List<DataTable> dataTables)
        {
            var testDirectory = Directory.CreateDirectory("CreateTestDirectoryWithTables" + DateTime.UtcNow.Ticks);
            dataTables = new List<DataTable>();

            for (var i = 0; i < 10; ++i)
            {
                var newTable = new DataTable
                {
                    TableFolderPath = testDirectory.FullName,
                    TableName = DateTime.UtcNow.Ticks + "_" + i
                };

                dataTables.Add(newTable);
            }

            foreach (var table in dataTables)
                File.WriteAllBytes(Path.Combine(table.TableFolderPath, table.TableName + ".db"), Resources.zakazky);

            return testDirectory;
        }

        private void OutputMatchesInput(string inputDirectory, string outputDirectory)
        {
            var dataTables = new DbScanner().GetDatabaseTables(inputDirectory);

            foreach (var table in dataTables)
            {
                var tablePath = Path.Combine(table.TableFolderPath, table.TableName + ".db");
                Assert.True(File.Exists(tablePath));

                var paradoxTable = new ParadoxTable(table.TableFolderPath, table.TableName);
                var csvTableRecords = File.ReadAllLines(Path.Combine(outputDirectory, table.TableName + ".csv"));

                // very basic check for now
                Assert.Equal(paradoxTable.RecordCount, csvTableRecords.Length);

                foreach (var record in csvTableRecords)
                {
                    var columnCount = record.Split(';').Length;

                    Assert.Equal(paradoxTable.FieldCount, columnCount);
                }
            }
        }

        [Fact]
        public void DbScannerFilteredListTest()
        {
            // GIVEN

            var testDirectoryInfo = CreateTestDirectoryWithTables(out var dataTables);
            var inputDirectory = testDirectoryInfo.FullName;
            var filteredTableList = dataTables.GetRange(0, 2);
            var filteredTableNames = filteredTableList.Select(dataTable => dataTable.TableName);

            //WHEN

            var scanner = new DbScanner();
            var actualDataTables = scanner.GetDatabaseTables(inputDirectory, filteredTableNames).ToList();

            //THEN

            AssertAreEqual(filteredTableList, actualDataTables);
        }

        [Fact]
        public void DbScannerFolderScanTest()
        {
            // GIVEN

            var testDirectory = CreateTestDirectoryWithTables(out var dataTables);

            // WHEN

            var scanner = new DbScanner();
            var actualDataTables = scanner.GetDatabaseTables(testDirectory.FullName).ToList();

            // THEN

            AssertAreEqual(dataTables, actualDataTables);
        }

        [Fact]
        public void DbWriterBasicTest()
        {
            // GIVEN

            var testDirectoryInfo = CreateTestDirectoryWithTables(out var _);
            var inputDirectory = testDirectoryInfo.FullName;
            var outputDirectory = Path.Combine(inputDirectory, "output");

            // WHEN

            var dbWriter = new DbWriter(new DbScanner(), outputDirectory, null, ";",
                false, false);
            dbWriter.WriteAll(inputDirectory);

            // THEN

            OutputMatchesInput(inputDirectory, outputDirectory);
        }
    }
}