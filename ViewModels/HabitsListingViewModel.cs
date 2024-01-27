using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using System.Collections.ObjectModel;

namespace HabitTracker.ViewModels
{
    public partial class HabitsListingViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        public ObservableCollection<Habit> Habits { get; set; } = new();

        public HabitsListingViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        [RelayCommand]
        public async Task GetHabits()
        {
            Habits.Clear();

            try
            {
                var habits = await _dataService.GetHabits();

                if (habits.Any())
                {
                    foreach (var habit in habits)
                    {
                        Habits.Add(habit);
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task AddHabit()
        {
            // Navigate to the add habit page
            await Shell.Current.GoToAsync("AddHabitPage");
        }


        [RelayCommand]
        private async Task DeleteBook(Habit habit)
        {
            var result = await Shell.Current.DisplayAlert("Delete", $"Are you sure you want to delete \"{habit.Name}\"?", "Yes", "No");

            if (result is true)
            {
                try
                {
                    await _dataService.DeleteHabit(habit.Id);
                    await GetHabits();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
                }
            }
        }
        [RelayCommand]
        private async Task OpenHabitDetail(Habit habit)
        {
            if (habit != null)
            {
                // Navigate to the detail page for the selected habit
                // The navigation depends on your routing setup, but here's a general example:
                var route = $"HabitDetailPage?HabitId={habit.Id}";
                await Shell.Current.GoToAsync(route);
            }
        }


        [RelayCommand]
        private async Task UpdateHabit(Habit habit) => await Shell.Current.GoToAsync("UpdateHabitPage", new Dictionary<string, object>
    {
        {"HabitObject", habit }
    });
    }
}