using System;
using System.Data.SQLite;
using System.IO;

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

            LoadSQLFiles(dbFilePath, dbConnectionString);
        }

        /// <summary>
        /// Load all of the SQL files in the SQLFileLocation, and run them against dbFilePath.
        /// </summary>
        /// <param name="dbFilePath"></param>
        private void LoadSQLFiles(string dbFilePath, string dbConnectionString)
        {
            // Get a list of files at that location.
            var sqlFileList = Directory.EnumerateFiles(SQLFileLocation);

            SQLiteConnection.CreateFile(dbFilePath);

            using (var connection = new SQLiteConnection(dbConnectionString))
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    connection.Open();

                    foreach (string filePath in sqlFileList)
                    {
                        //Console.WriteLine($"Loading SQL file: {filePath}");
                        cmd.CommandText = File.ReadAllText(filePath);
                        cmd.ExecuteNonQuery();
                    }

                }

                connection.Close();
            }
        }
    }
}
