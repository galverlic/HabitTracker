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



    }
}
