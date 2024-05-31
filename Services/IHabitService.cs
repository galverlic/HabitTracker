using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    public interface IHabitService
    {
        Task<IEnumerable<Habit>> GetHabitsForToday(Guid userId);
        Task AddHabitToUser(Habit habit, Guid userId);
        Task<IEnumerable<Habit>> GetHabitsByUserId(Guid userId);
        Task CreateHabit(Habit habit);
        Task DeleteHabit(Guid habitId);
        Task UpdateHabit(Habit habit);
        Task<HabitProgress> GetHabitProgress(Guid habitId, DateTime date);
        Task<List<HabitProgress>> GetProgressForHabitAsync(Guid habitId, DateTime date);
        Task AddOrUpdateHabitProgress(HabitProgress progress); // Already exists
        Task AddProgressAsync(HabitProgress progress); // Add this line
        Task ResetDailyHabits(Guid userId);
        Task<IEnumerable<Habit>> GetHabitsByDateAndUserId(DateTime date, Guid userId);
        Task DeleteHabitProgress(Guid habitProgressId); 
        Task<int> CalculateStreak(Guid habitId);
    }
}
