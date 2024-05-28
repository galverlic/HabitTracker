using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HabitTracker.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private User user;

        public UserProfileViewModel(IUserService userService)
        {
            _userService = userService;
        }

        [RelayCommand]
        public async Task LoadUserAsync()
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                User = await _userService.GetUserById(userId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load user data: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task LogOutAsync()
        {
            try
            {
                await _userService.LogOut();
                // Navigate to the login page or handle post-logout actions

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to log out: {ex.Message}");
            }
        }
    }
}
