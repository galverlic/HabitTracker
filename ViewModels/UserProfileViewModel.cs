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

        public IAsyncRelayCommand LoadUserProfileCommand { get; }
        public IAsyncRelayCommand LogOutCommand { get; }

        private async Task LoadUserProfile()
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(userId);

                UserName = user.Name;
                ProfilePictureUrl = user.ProfilePictureUrl;

                var habits = await _habitService.GetHabitsByUserId(userId);

                ActiveHabits.Clear();
                foreach (var habit in habits)
                {
                    habit.Streak = await _habitService.CalculateStreak(habit.HabitId);
                    ActiveHabits.Add(habit);
                }

                // Assuming we want the current streak of the longest streak habit
                CurrentStreak = ActiveHabits.Any() ? ActiveHabits.Max(h => h.Streak) : 0;
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
