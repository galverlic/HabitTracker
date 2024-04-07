using HabitTracker.Models;

namespace HabitTracker.Services
{
    public interface IHabitService
    {
        Task AddHabitToUser(Habit habit, Guid userId);
        Task<IEnumerable<Habit>> GetHabitsByUserId(Guid userId);
        Task<IEnumerable<Habit>> GetHabits();
        Task CreateHabit(Habit habit);
        Task DeleteHabit(Guid habitId);
        Task UpdateHabit(Habit habit);
    }
}
