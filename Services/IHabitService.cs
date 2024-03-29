using HabitTracker.Models;

namespace HabitTracker.Services
{
    public interface IHabitService
    {
        Task AddHabitToUser(Habit habit, int userId);
        Task<IEnumerable<Habit>> GetHabitsByUser(int userId);
        Task<IEnumerable<Habit>> GetHabits();
        Task CreateHabit(Habit habit);
        Task DeleteHabit(int id);
        Task UpdateHabit(Habit habit);
    }
}
