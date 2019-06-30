using MyWorkTracker.Code;
using MyWorkTracker.Data;

namespace DBBuilder
{
    class Program
    {

        static void Main(string[] args)
        {
            string dbConnectionString = "data source=" + @"D:\Development\Repos\MyWorkTracker\MyWorkTracker\bin\Debug\Data\MyWorkTracker.db";
            new DBInstaller(@"D:\Development\Repos\MyWorkTracker\MyWorkTracker\bin\Debug" + MWTModel.DatabaseFile, dbConnectionString);
        }
    }
}
