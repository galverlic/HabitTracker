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

                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No name!", "OK");
            }


        }
    }
}
