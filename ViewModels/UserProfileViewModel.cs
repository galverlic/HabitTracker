using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HabitTracker.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {
        private readonly IHabitService _habitService;
        private readonly IUserService _userService;

        public ObservableCollection<Habit> ActiveHabits { get; set; } = new ObservableCollection<Habit>();

        public UserProfileViewModel(IHabitService habitService, IUserService userService)
        {
            _habitService = habitService;
            _userService = userService;
            LoadUserProfileCommand = new AsyncRelayCommand(LoadUserProfile);
            LogOutCommand = new AsyncRelayCommand(LogOut);
        }

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string profilePictureUrl;

        [ObservableProperty]
        private int currentStreak;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int streak;

        public IAsyncRelayCommand LoadUserProfileCommand { get; }
        public IAsyncRelayCommand LogOutCommand { get; }

        private async Task LoadUserProfile()
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(userId);

                UserName = user.Name;
                ProfilePictureUrl = user.ProfilePictureUrl ?? "user.jpg"; // Use dynamic URL if available

                var habits = await _habitService.GetHabitsByUserId(userId);
                Debug.WriteLine($"Fetched {habits.Count()} habits for user {userId}");

                var activeHabits = new List<Habit>();

                foreach (var habit in habits)
                {
                    habit.Streak = await _habitService.CalculateStreak(habit.HabitId);
                    Debug.WriteLine($"Habit ID: {habit.HabitId}, Name: {habit.Name}, Streak: {habit.Streak}");

                    if (habit.Streak > 0)
                    {
                        activeHabits.Add(habit);
                    }
                }

                Debug.WriteLine($"Number of active habits: {activeHabits.Count}");

                // Clear and update ActiveHabits collection
                ActiveHabits.Clear();
                foreach (var habit in activeHabits)
                {
                    ActiveHabits.Add(habit);
                }

                // Assuming we want the current streak of the longest streak habit
                CurrentStreak = ActiveHabits.Any() ? ActiveHabits.Max(h => h.Streak) : 0;

                Debug.WriteLine($"Current streak: {CurrentStreak}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load user profile: {ex.Message}");
            }
        }

        private async Task LogOut()
        {
            try
            {
                await _userService.LogOut();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to log out: {ex.Message}");
            }
        }
    }
}
