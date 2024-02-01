using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;


namespace HabitTracker.ViewModels
{
    [QueryProperty(nameof(Habit), "HabitObject")]
    public partial class UpdateHabitViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Habit _habit;

        public UpdateHabitViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        [RelayCommand]
        private async Task UpdateHabit()
        {
            if (!string.IsNullOrEmpty(Habit.Name))
            {

                await _dataService.UpdateHabit(Habit);
                MessagingCenter.Send(this, "HabitUpdated");


                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No name!", "OK");
            }


        }

        [RelayCommand]
        private async Task DeleteHabit()
        {
            if (!string.IsNullOrEmpty(Habit.Name))
            {
                bool isConfirmed = await Shell.Current.DisplayAlert("Confirm Delete",
                                                                    $"Are you sure you want to delete \"{Habit.Name}\"?",
                                                                    "Yes", "No");

                if (isConfirmed)
                {
                    await _dataService.DeleteHabit(Habit.Id);
                    MessagingCenter.Send(this, "HabitDeleted");
                    await Shell.Current.GoToAsync("..");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Habit name is not specified.", "OK");
            }
        }

        [RelayCommand]
        private async Task ResetStreak()
        {
            // Check if the Habit's Name is not empty or null
            if (!string.IsNullOrEmpty(Habit.Name))
            {
                // Ask the user for confirmation to reset the streak
                bool isConfirmed = await Shell.Current.DisplayAlert(
                    "Confirm Streak Reset",
                    $"Are you sure you want to reset your streak for \"{Habit.Name}\"?",
                    "Yes", "No"
                );

                // If the user confirms, reset the streak
                if (isConfirmed)
                {
                    Habit.Streak = 0;
                    await _dataService.UpdateHabit(Habit);
                    MessagingCenter.Send(this, "HabitStreakReset");
                    await Shell.Current.GoToAsync(".."); // Navigate back
                }
            }
            else
            {
                // If Habit.Name is empty or null, show an error message
                await Shell.Current.DisplayAlert("Error", "Habit name is not specified.", "OK");
            }
        }

    }

}

