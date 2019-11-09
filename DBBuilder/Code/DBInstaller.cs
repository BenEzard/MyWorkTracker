using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;

namespace MyWorkTracker.Data
{
    /// <summary>
    /// This method can be used to initialise the database.
    /// </summary>
    class DBInstaller
    {
        /// <summary>
        /// The path where all of the SQL files that should be executed are.
        /// Their names should confer the run-order.
        /// </summary>
        public const string SQLFileLocation = @"D:\Development\Repos\MyWorkTracker\DBBuilder\SQL";

        public DBInstaller(string dbFilePath, string dbConnectionString)
        {
            Console.WriteLine($"Overwriting the file {dbFilePath}");

            SQLiteConnection.CreateFile(dbFilePath);

            LoadSQLFiles(dbFilePath, dbConnectionString);
            InsertRecords(dbFilePath, dbConnectionString);
        }

        private void InsertRecords(string dbFilePath, string dbConnectionString)
        {
            // Get a list of files at that location.
            var sqlFileList = Directory.EnumerateFiles(SQLFileLocation, "*.recs");

            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();

                    foreach (string filePath in sqlFileList)
                    {
                        Console.WriteLine($"Loading records: {filePath}");
                        string fileContents = File.ReadAllText(filePath);
                        string[] inserts = fileContents.Split(new string[] { "INSERT INTO " }, StringSplitOptions.None);
                        Console.WriteLine("Number of records: " + inserts.Length);
                        foreach (string token in inserts) {
                            if (token.Length > 0)
                            {
//                                Console.WriteLine($"> {token}");
                                string sql = "INSERT INTO " + token;
                                cmd.CommandText = sql;
                                Console.WriteLine(sql);
                                Thread.Sleep(500);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                connection.Close();
            }
        }
    

        /// <summary>
        /// Load all of the SQL files in the SQLFileLocation, and run them against dbFilePath.
        /// </summary>
        /// <param name="dbFilePath"></param>
        private void LoadSQLFiles(string dbFilePath, string dbConnectionString)
        {
            // Get a list of files at that location.
            var sqlFileList = Directory.EnumerateFiles(SQLFileLocation, "*.sql");

            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();

                    foreach (string filePath in sqlFileList)
                    {
                        Console.WriteLine($"Loading SQL file: {filePath}");
                        cmd.CommandText = File.ReadAllText(filePath);
                        cmd.ExecuteNonQuery();
                    }

                }

                connection.Close();
            }
        }
    }
}
