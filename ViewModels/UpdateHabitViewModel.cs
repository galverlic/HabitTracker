using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HabitTracker.ViewModels
{
    [QueryProperty(nameof(HabitJson), "HabitObject")]
    [QueryProperty(nameof(SelectedDate), "SelectedDate")]
    public partial class UpdateHabitViewModel : ObservableObject
    {
        private readonly IHabitService _habitService;

        [ObservableProperty]
        private Habit habit;

        [ObservableProperty]
        private DateTime selectedDate;

        private string habitJson;
        public string HabitJson
        {
            get => habitJson;
            set
            {
                habitJson = value;
                try
                {
                    Habit = JsonConvert.DeserializeObject<Habit>(Uri.UnescapeDataString(habitJson));
                    RefreshFrequencyChecks();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deserializing HabitJson: {ex.Message}");
                }
            }
        }

        public UpdateHabitViewModel(IHabitService habitService)
        {
            _habitService = habitService;
        }

        public bool IsMondayChecked
        {
            get => Habit?.Frequency.HasFlag(Habit.HabitFrequency.Monday) ?? false;
            set => SetDayFlag(Habit.HabitFrequency.Monday, value);
        }

        public bool IsTuesdayChecked
        {
            get => Habit?.Frequency.HasFlag(Habit.HabitFrequency.Tuesday) ?? false;
            set => SetDayFlag(Habit.HabitFrequency.Tuesday, value);
        }

        public bool IsWednesdayChecked
        {
            get => Habit?.Frequency.HasFlag(Habit.HabitFrequency.Wednesday) ?? false;
            set => SetDayFlag(Habit.HabitFrequency.Wednesday, value);
        }

        public bool IsThursdayChecked
        {
            get => Habit?.Frequency.HasFlag(Habit.HabitFrequency.Thursday) ?? false;
            set => SetDayFlag(Habit.HabitFrequency.Thursday, value);
        }

        public bool IsFridayChecked
        {
            get => Habit?.Frequency.HasFlag(Habit.HabitFrequency.Friday) ?? false;
            set => SetDayFlag(Habit.HabitFrequency.Friday, value);
        }

        public bool IsSaturdayChecked
        {
            get => Habit?.Frequency.HasFlag(Habit.HabitFrequency.Saturday) ?? false;
            set => SetDayFlag(Habit.HabitFrequency.Saturday, value);
        }

        public bool IsSundayChecked
        {
            get => Habit?.Frequency.HasFlag(Habit.HabitFrequency.Sunday) ?? false;
            set => SetDayFlag(Habit.HabitFrequency.Sunday, value);
        }

        private void SetDayFlag(Habit.HabitFrequency day, bool isChecked)
        {
            if (Habit == null) return;

            if (isChecked)
                Habit.Frequency |= day;
            else
                Habit.Frequency &= ~day;
            OnPropertyChanged(nameof(Habit.Frequency));
        }

        private void RefreshFrequencyChecks()
        {
            OnPropertyChanged(nameof(IsMondayChecked));
            OnPropertyChanged(nameof(IsTuesdayChecked));
            OnPropertyChanged(nameof(IsWednesdayChecked));
            OnPropertyChanged(nameof(IsThursdayChecked));
            OnPropertyChanged(nameof(IsFridayChecked));
            OnPropertyChanged(nameof(IsSaturdayChecked));
            OnPropertyChanged(nameof(IsSundayChecked));
        }

        [RelayCommand]
        private async Task UpdateHabit()
        {
            if (!string.IsNullOrEmpty(Habit.Name))
            {
                try
                {
                    Habit.Frequency = CalculateFrequency();
                    await _habitService.UpdateHabit(Habit);
                    MessagingCenter.Send(this, "HabitUpdated");
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating habit: {ex.Message}");
                    await Shell.Current.DisplayAlert("Error", "Failed to update habit.", "OK");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No name!", "OK");
            }
        }

        private Habit.HabitFrequency CalculateFrequency()
        {
            Habit.HabitFrequency frequency = Habit.HabitFrequency.None;

            if (IsMondayChecked) frequency |= Habit.HabitFrequency.Monday;
            if (IsTuesdayChecked) frequency |= Habit.HabitFrequency.Tuesday;
            if (IsWednesdayChecked) frequency |= Habit.HabitFrequency.Wednesday;
            if (IsThursdayChecked) frequency |= Habit.HabitFrequency.Thursday;
            if (IsFridayChecked) frequency |= Habit.HabitFrequency.Friday;
            if (IsSaturdayChecked) frequency |= Habit.HabitFrequency.Saturday;
            if (IsSundayChecked) frequency |= Habit.HabitFrequency.Sunday;

            return frequency;
        }

        [RelayCommand]
        private async Task DeleteHabit()
        {
            Debug.WriteLine("DeleteHabit command executed");

            var action = await Shell.Current.DisplayActionSheet(
                $"Delete {Habit.Name}",
                "Cancel",
                null,
                "Delete This Day's Habit",
                "Delete All Future Instances");

            Debug.WriteLine($"User selected action: {action}");

            if (action == "Delete This Day's Habit")
            {
                await DeleteSingleHabitInstance(SelectedDate);
            }
            else if (action == "Delete All Future Instances")
            {
                await DeleteAllHabitInstances();
            }
        }

        private async Task DeleteSingleHabitInstance(DateTime date)
        {
            Debug.WriteLine($"Attempting to delete single instance of habit: {Habit.HabitId} on {date}");

            var progress = await _habitService.GetHabitProgress(Habit.HabitId, date);
            if (progress != null)
            {
                Debug.WriteLine($"Found habit progress to delete: {progress.ProgressId}");
                await _habitService.DeleteHabitProgress(progress.ProgressId);
            }
            else
            {
                await _habitService.DeleteHabit(Habit.HabitId);
                Debug.WriteLine("No habit progress found for the specified date.");
            }
        }

        private async Task DeleteAllHabitInstances()
        {
            try
            {
                await _habitService.DeleteHabit(Habit.HabitId);
                MessagingCenter.Send(this, "HabitDeleted");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting all habit instances: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to delete habit.", "OK");
            }
        }

        [RelayCommand]
        private async Task ResetStreak()
        {
            if (!string.IsNullOrEmpty(Habit.Name))
            {
                bool isConfirmed = await Shell.Current.DisplayAlert(
                    "Confirm Streak Reset",
                    $"Are you sure you want to reset your streak for \"{Habit.Name}\"?",
                    "Yes", "No"
                );

                if (isConfirmed)
                {
                    Habit.Streak = 0;
                    await _habitService.UpdateHabit(Habit);
                    MessagingCenter.Send(this, "HabitStreakReset");
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
