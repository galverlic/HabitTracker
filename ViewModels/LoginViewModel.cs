using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Services;

namespace HabitTracker.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string userEmail;
        [ObservableProperty]
        private string userPassword;

        private readonly IUserService _userService;


        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
        }

        [RelayCommand]
        public async Task AttemptLogin()
        {
            bool isSuccess = await _userService.LogIn(UserEmail, UserPassword);
            if (isSuccess)
            {
                await Shell.Current.GoToAsync("/HabitsListingPage");

            }
            else
            {
                await Shell.Current.DisplayAlert("Login Failed", "Incorrect email or password.", "OK");

            }
        }

        [RelayCommand]
        public async Task LoginWithGoogle()
        {
            try
            {
                bool isSuccess = await _userService.LogInWithGoogle();
                if (isSuccess)
                {
                    await Shell.Current.GoToAsync("HabitsListingPage");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Login Failed", "Google Sign-In failed.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task NavigateToRegister()
        {
            await Shell.Current.GoToAsync("RegistrationPage", true);
        }
    }
}
