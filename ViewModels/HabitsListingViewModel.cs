using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace HabitTracker.ViewModels
{
    public partial class HabitsListingViewModel : ObservableObject
    {
        private readonly IHabitService _habitService;
        private readonly IUserService _userService;

        private bool _isPopupVisible;

        private string _userRealName;
        public string UserRealName
        {
            get => _userRealName;
            set => SetProperty(ref _userRealName, value);
        }


        public ObservableCollection<Habit> Habits { get; set; } = new ObservableCollection<Habit>();

        [ObservableProperty]
        private string currentDateDisplay;

        public HabitsListingViewModel(IHabitService habitService, IUserService userService)
        {
            _habitService = habitService;
            _userService = userService;
            InitializeUser();

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

            MessagingCenter.Subscribe<UpdateHabitViewModel>(this, "HabitStreakReset", (sender) =>
            {
                GetHabitsCommand.Execute(null);
            });

            ClosePopupCommand = new Command(() => IsPopupVisible = false);

            GetHabits();

        }

        public async Task InitializeUser()
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(userId);
                UserRealName = user.Name;  // Set the user's real name
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load user data: " + ex.Message);
                UserRealName = "Default Name";  // Default name if fail to load
            }
        }


        [RelayCommand]
        public async Task ViewHabit(Habit habit)
        {
            if (habit != null)
            {
                // Navigate to the UpdateHabitPage with the selected habit
                var route = $"UpdateHabitPage?HabitId={habit.UserId}";
                await Shell.Current.GoToAsync(route);
            }
        }
        [RelayCommand]
        public async Task SelectHabit(Habit selectedHabit)
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
                await _habitService.UpdateHabit(habit);
                // Refresh the list or handle UI updates as necessary
                await GetHabits();

            }
        }

        // once a day it should run the program a method to reset the streak but save it before

        [RelayCommand]
        public async Task IncrementRepetition(Habit habit)
        {
            if (habit != null)
            {
                habit.CurrentRepetition++;
                if (habit.CurrentRepetition >= habit.TargetRepetition)
                {
                    habit.Streak++;
                    habit.IsCompleted = true;
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    string text = $"Congrats! You finished {habit.Name}. \nStreak: {habit.Streak} {(habit.Streak == 1 ? "day" : "days")}";
                    ToastDuration duration = ToastDuration.Long;
                    double fontSize = 14;

                    var toast = Toast.Make(text, duration, fontSize);

                    await toast.Show(cancellationTokenSource.Token);

                }
                await _habitService.UpdateHabit(habit);
                await GetHabits();
            }
        }
        [RelayCommand]
        public async Task DecrementRepetition(Habit habit)
        {
            try
            {
                if (habit.CurrentRepetition <= 0)
                {
                    habit.CurrentRepetition = 0;
                }
                else
                {
                    habit.CurrentRepetition--;
                }
                await _habitService.UpdateHabit(habit);
                await GetHabits();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DecrementRepetition Error: {ex.Message}");
            }
        }

        public bool IsPopupVisible
        {
            get => _isPopupVisible;
            set => SetProperty(ref _isPopupVisible, value);
        }

        public void ShowPopup()
        {
            IsPopupVisible = true;
        }

        public ICommand ClosePopupCommand { get; }


        [RelayCommand]
        public async Task GetHabits()
        {
            Habits.Clear();
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var habits = await _habitService.GetHabitsByUserId(userId);
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
                var route = $"HabitDetailPage?HabitId={habit.UserId}";
                await Shell.Current.GoToAsync(route);
            }
        }
        public async Task UpdateHabitCompletionStatus(Habit habit, bool isChecked)
        {
            if (habit != null)
            {
                habit.IsCompleted = isChecked;
                await _habitService.UpdateHabit(habit);
                // Refresh the list or handle UI updates
                await GetHabits();
            }
        }

        public void ShowNotifications()
        {
            MessagingCenter.Send(this, "ShowNotification");
        }
        public void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<AddHabitViewModel>(this, "HabitAdded");
            MessagingCenter.Unsubscribe<UpdateHabitViewModel>(this, "HabitUpdated");
            MessagingCenter.Unsubscribe<UpdateHabitViewModel>(this, "HabitDeleted");
            MessagingCenter.Unsubscribe<UpdateHabitViewModel>(this, "HabitSteakReset");


        }
    }
}