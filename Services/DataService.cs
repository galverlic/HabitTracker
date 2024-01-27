using HabitTracker.Models;
using Supabase;

namespace HabitTracker.Services
{
    public class DataService : IDataService
    {
        private readonly Client _supabaseClient;

        public DataService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
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

            // Check if ReminderTime is not the default value
            //if (habit.ReminderTime != default(DateTime))
            //    updateQuery = updateQuery.Set(h => h.ReminderTime, habit.ReminderTime);

            // Check if StartDate is not the default value
            if (habit.StartDate != default(DateTime))
                updateQuery = updateQuery.Set(h => h.StartDate, habit.StartDate);

            // Assuming IsCompleted is a non-nullable boolean
            updateQuery = updateQuery.Set(h => h.IsCompleted, habit.IsCompleted);

            // Check if Streak is not the default value (assuming it's a non-nullable int)
            if (habit.Streak != default(int))
                updateQuery = updateQuery.Set(h => h.Streak, habit.Streak);

            // Perform the update
            await updateQuery.Update();
        }



    }
}
