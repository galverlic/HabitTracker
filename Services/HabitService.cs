using HabitTracker.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HabitTracker.Services
{
    public class HabitService : IHabitService
    {
        private readonly SQLiteAsyncConnection _db;

        public HabitService()
        {
            // Utilize the Constants.DatabasePath directly
            _db = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            _db.CreateTableAsync<Habit>().Wait();
        }

        public async Task AddHabitToUser(Habit habit, Guid userId)
        {
            habit.UserId = userId;
            await _db.InsertAsync(habit);
        }

        public async Task<IEnumerable<Habit>> GetHabitsByUserId(Guid userId)
        {
            return await _db.Table<Habit>().Where(h => h.UserId == userId).ToListAsync();
        }


        public async Task<IEnumerable<Habit>> GetHabits()
        {
            return await _db.Table<Habit>().OrderByDescending(h => h.UserId).ToListAsync();
        }

        public async Task CreateHabit(Habit habit)
        {
            await _db.InsertAsync(habit);
        }

        public async Task DeleteHabit(Guid id)
        {
            var habit = await _db.FindAsync<Habit>(id);
            if (habit != null)
            {
                await _db.DeleteAsync(habit);
            }
        }

        public async Task UpdateHabit(Habit habit)
        {
            await _db.UpdateAsync(habit);
        }

        // Timer-related methods omitted for brevity
    }
}

        //private void SetUpTimer()
        //{
        //    // Calculate time until next midnight
        //    var now = DateTime.Now;
        //    var nextMidnight = now.Date.AddDays(1);
        //    var millisecondsUntilMidnight = (nextMidnight - now).TotalMilliseconds;

        //    // Set up the timer
        //    _timer = new System.Timers.Timer(1000);
        //    _timer.Elapsed += OnMidnightReached;
        //    _timer.AutoReset = false; // Ensure it doesn't auto-reset; we'll manually reset it
        //    _timer.Start();
        //}

        //private void OnMidnightReached(object sender, ElapsedEventArgs e)
        //{

        //    // Reset the streak here
        //    ResetDailyStreak();

        //    // Recalculate the timer for the next day
        //    SetUpTimer();
        //}

        //private async void ResetDailyStreak()
        //{
        //    var habits = GetHabits().Result;

        //    foreach (var habit in habits)
        //    {
        //        Console.WriteLine("Streak: " + habit.Streak);

        //        if (habit.CurrentRepetition >= habit.TargetRepetition)
        //        {
        //            habit.Streak++;
        //        }
        //        else
        //        {
        //            habit.Streak = 0;
        //        }

        //        habit.CurrentRepetition = 0;

        //        await UpdateHabit(habit);
        //    }

        //}




    

