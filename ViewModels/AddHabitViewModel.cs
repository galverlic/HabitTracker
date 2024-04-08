using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.ViewModels
{
    public partial class AddHabitViewModel : ObservableObject
    {
        private readonly IHabitService _habitService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private string _habitName;
        [ObservableProperty]
        private string _habitDescription;
        [ObservableProperty]
        private string _habitFrequency;
        [ObservableProperty]
        private bool _habitIsCompleted;
        [ObservableProperty]
        private int _habitTargetRepetition;

        

        public AddHabitViewModel(IHabitService habitService, IUserService userService)
        {
            _habitService = habitService;
            _userService = userService;
        }

        [RelayCommand]
        private async Task AddHabit()
        {
            try
            {
                if (!string.IsNullOrEmpty(HabitName))
                {
                    Habit habit = new()
                    {
                        Name = HabitName,
                        Description = HabitDescription,
                        Frequency = HabitFrequency,
                        TargetRepetition = HabitTargetRepetition,

                    };
                    await _habitService.CreateHabit(habit);
                    // Inside AddHabit method after adding the habit
                    MessagingCenter.Send(this, "HabitAdded");

                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "No name!", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}

/*
 TODO:
--> Add habits already pre-did, like "Drink water" with the icon , "Make bed", "Exercise",



 */