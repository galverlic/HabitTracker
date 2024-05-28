using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HabitTracker.ViewModels
{
    [QueryProperty(nameof(Habit), "HabitObject")]
    public partial class UpdateHabitViewModel : ObservableObject
    {
        private readonly IHabitService _habitService;

        [ObservableProperty]
        private Habit habit;

        private string habitJson;
        public string HabitJson
        {
            get => habitJson;
            set
            {
                habitJson = value;
                Habit = JsonConvert.DeserializeObject<Habit>(habitJson);
                OnPropertyChanged(nameof(IsMondayChecked));
                OnPropertyChanged(nameof(IsTuesdayChecked));
                OnPropertyChanged(nameof(IsWednesdayChecked));
                OnPropertyChanged(nameof(IsThursdayChecked));
                OnPropertyChanged(nameof(IsFridayChecked));
                OnPropertyChanged(nameof(IsSaturdayChecked));
                OnPropertyChanged(nameof(IsSundayChecked));
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
            if (isChecked)
                Habit.Frequency |= day;
            else
                Habit.Frequency &= ~day;
            OnPropertyChanged(nameof(Habit.Frequency));
        }

        [RelayCommand]
        private async Task UpdateHabit()
        {
            if (!string.IsNullOrEmpty(Habit.Name))
            {
                await _habitService.UpdateHabit(Habit);
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
                    await _habitService.DeleteHabit(Habit.HabitId);
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
