using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParadoxDbCopier.IO;
using ParadoxDbCopierTests.data;
using ParadoxReader;

namespace ParadoxDbCopierTests
{
    [TestClass]
    public class UnitTests
    {
        public TestContext TestContext { get; set; }

        private void AssertAreEqual(List<DataTable> expectedDataTables, List<DataTable> actualDataTables)
        {
            Assert.AreEqual(expectedDataTables.Count, actualDataTables.Count);

            foreach (var table in expectedDataTables) Assert.IsTrue(actualDataTables.Contains(table));
        }

        private DirectoryInfo CreateTestDirectoryWithTables(out List<DataTable> dataTables)
        {
            var testDirectory = Directory.CreateDirectory(TestContext.TestName + DateTime.UtcNow.Ticks);
            dataTables = new List<DataTable>();

            for (var i = 0; i < 10; ++i)
            {
                var newTable = new DataTable
                {
                    TableFolderPath = testDirectory.FullName,
                    TableName = DateTime.UtcNow.Ticks.ToString() + "_" + i 
                };

                dataTables.Add(newTable);
            }

            foreach (var table in dataTables)
            {
                File.WriteAllBytes(Path.Combine(table.TableFolderPath, table.TableName + ".db"), Resources.zakazky);
            }

            return testDirectory;
        }

        private void OutputMatchesInput(string inputDirectory, string outputDirectory)
        {
            var dataTables = new DbScanner().GetDatabaseTables(inputDirectory);

            foreach (var table in dataTables)
            {
                var tablePath = Path.Combine(table.TableFolderPath, table.TableName);
                Assert.IsTrue(File.Exists(tablePath));

                var paradoxTable = new ParadoxTable(table.TableFolderPath, table.TableName);
                var csvTableRecords = File.ReadAllLines(Path.Combine(outputDirectory, table.TableName + ".csv"));

                // very basic check for now
                Assert.AreEqual(paradoxTable.RecordCount, csvTableRecords.Length,
                    "The number of records written is not equal to the number of records read");

                foreach (var record in csvTableRecords)
                {
                    var columnCount = record.Split(';').Length;

                    Assert.AreEqual(paradoxTable.FieldCount, columnCount,
                        "The number of columns writtens is not equal to the number of records read");
                }
            }
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void DbWriterBasicTest()
        {
            // GIVEN

            var testDirectoryInfo = CreateTestDirectoryWithTables(out var _);
            var inputDirectory = testDirectoryInfo.FullName;
            var outputDirectory = Path.Combine(inputDirectory, "output");

            // WHEN

            var dbWriter = new DbWriter(new DbScanner(), outputDirectory, null, ";",
                true, false);
            dbWriter.WriteAll(inputDirectory);

            // THEN

            OutputMatchesInput(inputDirectory, outputDirectory);
        }
    }
}