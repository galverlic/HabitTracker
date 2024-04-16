using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HabitTracker.Models;

namespace HabitTracker.Data
{
    public class HabitTrackerDatabase
    {
        SQLiteAsyncConnection Database;

        public HabitTrackerDatabase()
        {

        }

        async Task Init()
        {
            if (Database != null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await Database.CreateTableAsync<User>();
            await Database.CreateTableAsync<Habit>();
        }
    }
}
