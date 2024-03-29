using HabitTracker.Models;
using Supabase;
using System.Timers;

namespace HabitTracker.Services
{
    public class HabitService : IHabitService
    {
        private readonly Client _supabaseClient;
        private System.Timers.Timer _timer;

        public HabitService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
            SetUpTimer();

        }

        public async Task AddHabitToUser(Habit habit, int userId)
        {
            habit.UserId = userId;
            await _supabaseClient.From<Habit>().Insert(habit);
        }

        public async Task<IEnumerable<Habit>> GetHabitsByUser(int userId)
        {
            var response = await _supabaseClient.From<Habit>().Filter("user_id", Postgrest.Constants.Operator.Equals, userId).Get();
            return response.Models;
        }

        public async Task<IEnumerable<Habit>> GetHabits()
        {
            var response = await _supabaseClient.From<Habit>().Get();
            return response.Models.OrderByDescending(h => h.Id);
        }

        public async Task CreateHabit(Habit habit)
        {
            await _supabaseClient.From<Habit>().Insert(habit);
        }

        public async Task DeleteHabit(int id)
        {
            await _supabaseClient.From<Habit>()
                .Where(h => h.Id == id).Delete();
        }
        public async Task UpdateHabit(Habit habit)
        {
            var updateQuery = _supabaseClient.From<Habit>().Where(h => h.Id == habit.Id);

            if (habit.Name != null)
                updateQuery = updateQuery.Set(h => h.Name, habit.Name);

            if (habit.Description != null)
                updateQuery = updateQuery.Set(h => h.Description, habit.Description);

            if (habit.Frequency != null)
                updateQuery = updateQuery.Set(h => h.Frequency, habit.Frequency);

            if (habit.CurrentRepetition >= 0)
                updateQuery = updateQuery.Set(h => h.CurrentRepetition, habit.CurrentRepetition);

            // Check if ReminderTime is not the default value
            //if (habit.ReminderTime != default(DateTime))
            //    updateQuery = updateQuery.Set(h => h.ReminderTime, habit.ReminderTime);

            if (habit.TargetRepetition != 0)
                updateQuery = updateQuery.Set(h => h.TargetRepetition, habit.TargetRepetition);

            // Check if StartDate is not the default value
            if (habit.StartDate != default(DateTime))
                updateQuery = updateQuery.Set(h => h.StartDate, habit.StartDate);

            // Assuming IsCompleted is a non-nullable boolean
            updateQuery = updateQuery.Set(h => h.IsCompleted, habit.IsCompleted);

            // Check if Streak is not the default value (assuming it's a non-nullable int)
            if (habit.Streak >= 0)
                updateQuery = updateQuery.Set(h => h.Streak, habit.Streak);

            // Perform the update
            await updateQuery.Update();
        }

        private void SetUpTimer()
        {
            // Calculate time until next midnight
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            var millisecondsUntilMidnight = (nextMidnight - now).TotalMilliseconds;

            // Set up the timer
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnMidnightReached;
            _timer.AutoReset = false; // Ensure it doesn't auto-reset; we'll manually reset it
            _timer.Start();
        }

        private void OnMidnightReached(object sender, ElapsedEventArgs e)
        {

            // Reset the streak here
            ResetDailyStreak();

            // Recalculate the timer for the next day
            SetUpTimer();
        }

        private async void ResetDailyStreak()
        {
            var habits = GetHabits().Result;

            foreach (var habit in habits)
            {
                Console.WriteLine("Streak: " + habit.Streak);

                if (habit.CurrentRepetition >= habit.TargetRepetition)
                {
                    habit.Streak++;
                }
                else
                {
                    habit.Streak = 0;
                }

                habit.CurrentRepetition = 0;

                await UpdateHabit(habit);
            }

        }




    }
}
