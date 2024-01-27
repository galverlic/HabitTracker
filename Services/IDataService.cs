using HabitTracker.Models;

namespace HabitTracker.Services
{
    public interface IDataService
    {
        Task<IEnumerable<Habit>> GetHabits();
        Task CreateHabit(Habit habit);
        Task DeleteHabit(int id);
        Task UpdateHabit(Habit habit);
    }
}
