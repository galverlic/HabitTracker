using HabitTracker.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HabitTracker.Services
{
    public interface IHabitService
    {
        Task AddHabitToUser(Habit habit, Guid userId);  // Adds a new habit associated with a specific user
        Task<IEnumerable<Habit>> GetHabitsByUserId(Guid userId);  // Retrieves all habits for a specific user
        Task CreateHabit(Habit habit);  // Creates a new habit record in the database
        Task DeleteHabit(Guid habitId);  // Deletes a specific habit by its ID
        Task UpdateHabit(Habit habit);  // Updates a specific habit's details
    }
}
