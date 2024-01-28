using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace HabitTracker.ViewModels
{
    public partial class HabitsListingViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        public ObservableCollection<Habit> Habits { get; set; } = new ObservableCollection<Habit>();

        [ObservableProperty]
        private string currentDateDisplay;

        public HabitsListingViewModel(IDataService dataService)
        {
            _dataService = dataService;
            CurrentDateDisplay = DateTime.Now.ToString("D"); // Sets the current date display

            MessagingCenter.Subscribe<AddHabitViewModel>(this, "HabitAdded", (sender) =>
            {
                GetHabitsCommand.Execute(null);
            });

            MessagingCenter.Subscribe<UpdateHabitViewModel>(this, "HabitUpdated", (sender) =>
            {
                GetHabitsCommand.Execute(null);
            });

            MessagingCenter.Subscribe<UpdateHabitViewModel>(this, "HabitDeleted", (sender) =>
            {
                GetHabitsCommand.Execute(null);
            });
            GetHabits();

        }
        [RelayCommand]
        public async Task ViewHabit(Habit habit)
        {
            if (habit != null)
            {
                // Navigate to the UpdateHabitPage with the selected habit
                var route = $"UpdateHabitPage?HabitId={habit.Id}";
                await Shell.Current.GoToAsync(route);
            }
        }
        [RelayCommand]
        private async Task SelectHabit(Habit selectedHabit)
        {
            if (selectedHabit != null)
            {
                // Use the same key as in the QueryProperty of UpdateHabitViewModel
                var navigationParameter = new Dictionary<string, object>
        {
            { "HabitObject", selectedHabit }
        };
                await Shell.Current.GoToAsync($"UpdateHabitPage", navigationParameter);
            }
        }

        [RelayCommand]
        public async Task ToggleHabitCompleted(Habit habit)
        {
            if (habit != null)
            {
                habit.IsCompleted = !habit.IsCompleted;
                await _dataService.UpdateHabit(habit);
                // Refresh the list or handle UI updates as necessary
                await GetHabits();

            }
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
                Debug.WriteLine($"Error in GetHabits: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load habits.", "OK");
            }
        }



        [RelayCommand]
        public async Task AddHabit()
        {
            // Navigate to the add habit page
            await Shell.Current.GoToAsync("AddHabitPage");
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
        public async Task UpdateHabitCompletionStatus(Habit habit, bool isChecked)
        {
            if (habit != null)
            {
                habit.IsCompleted = isChecked;
                await _dataService.UpdateHabit(habit);
                // Refresh the list or handle UI updates
                await GetHabits();
            }
        }


        public void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<AddHabitViewModel>(this, "HabitAdded");
            MessagingCenter.Unsubscribe<UpdateHabitViewModel>(this, "HabitUpdated");
            MessagingCenter.Unsubscribe<UpdateHabitViewModel>(this, "HabitDeleted");


        }
    }
}