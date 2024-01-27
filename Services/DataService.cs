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
            await _supabaseClient.From<Habit>().Where(h => h.Id == habit.Id)
                .Set(h => h.Name, habit.Name)
                .Set(h => h.Description, habit.Description)
                .Set(h => h.Frequency, habit.Frequency)
                .Set(h => h.ReminderTime, habit.ReminderTime)
                .Set(h => h.StartDate, habit.StartDate)
                .Set(h => h.Status, habit.Status)
                .Set(h => h.Streak, habit.Streak)
                .Update();

        }
    }
}
