using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Labs.SightsToSee.Library.Models;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Attributes;
using Microsoft.Labs.SightsToSee.Models;

namespace Microsoft.Labs.SightsToSee.Library.Services.DataModelService
{
    public static class SQLiteService
    {
        public static SQLiteConnection CreateConnection()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "sightstosee.sqlite");
            return new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path, storeDateTimeAsTicks:true);
        }

        public static SQLiteAsyncConnection CreateAsyncConnection()
        {
            return new SQLiteAsyncConnection(SqliteConnectionFunc);
        }

        private static SQLiteConnectionWithLock SqliteConnectionFunc()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "sightstosee.sqlite");
            return new SQLiteConnectionWithLock(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), new SQLiteConnectionString(path, true));
        }


        public static void ClearLocalDb()
        {
            using (var conn = CreateConnection())
            {
                conn.DropTable<Trip>();
                conn.DropTable<Sight>();
                conn.DropTable<SightFile>();
                conn.Commit();
            }
        }

        public static async Task InitDb()
        {
            using (var conn = CreateConnection())
            {
                // Create tables - does nothing if the tables already exist (except add missing columns)
                conn.CreateTable<SightFile>();
                conn.CreateTable<Sight>();
                conn.CreateTable<Trip>();

            // seed the statuses
               await InsertTestDataAsync(conn);
            }
        }

        private static async Task InsertTestDataAsync(SQLiteConnection conn)
        {
            await SeedDataFactory.LoadDataAsync(conn);
            AppSettings.LastTripId = SeedDataFactory.TripId;
        }
    }
}