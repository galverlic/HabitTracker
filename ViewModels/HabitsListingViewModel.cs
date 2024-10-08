using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HabitTracker.ViewModels
{
    public partial class HabitsListingViewModel : ObservableObject
    {
        private readonly IHabitService _habitService;
        private readonly IUserService _userService;

        private DateTime _currentDate;
        private bool _isPopupVisible;
        private string _userRealName;

        public string UserRealName
        {
            get => _userRealName;
            set => SetProperty(ref _userRealName, value);
        }

        public ObservableCollection<Habit> Habits { get; set; } = new ObservableCollection<Habit>();
        public ObservableCollection<HabitProgress> HabitProgresses { get; set; } = new ObservableCollection<HabitProgress>();

        public DateTime CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged();
                CurrentDateDisplay = _currentDate.ToString("D");
                LoadHabitsCommand.Execute(null);
            }
        }

        [ObservableProperty]
        private bool isPopupVisible;

        public ICommand LoadHabitsCommand { get; }
        public ICommand AddProgressCommand { get; }
        public ICommand GoToPreviousDayCommand { get; }
        public ICommand GoToNextDayCommand { get; }
        public ICommand ClosePopupCommand { get; }

        [ObservableProperty]
        private string currentDateDisplay;

        public HabitsListingViewModel(IHabitService habitService, IUserService userService)
        {
            _habitService = habitService;
            _userService = userService;

            // Initialize CurrentDate to today's date
            _currentDate = DateTime.Now.Date;
            CurrentDateDisplay = _currentDate.ToString("D");

            LoadHabitsCommand = new Command(async () => await LoadHabits());
            AddProgressCommand = new Command<Guid>(async (habitId) => await AddProgress(habitId));
            GoToPreviousDayCommand = new Command(() => CurrentDate = CurrentDate.AddDays(-1));
            GoToNextDayCommand = new Command(() => CurrentDate = CurrentDate.AddDays(1));
            ClosePopupCommand = new Command(() => IsPopupVisible = false);

            MessagingCenter.Subscribe<UserService>(this, "UserLoggedIn", async (sender) =>
            {
                await LoadHabits();
            });
            MessagingCenter.Subscribe<HabitService, Habit>(this, "HabitProgressUpdated", async (sender, habit) =>
            {
                await RefreshHabits();
            });
        }

        public async Task InitializeUser()
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(userId);
                UserRealName = user.Name;
                await LoadHabits();
            }
            catch (InvalidOperationException invEx)
            {
                Debug.WriteLine("User not logged in: " + invEx.Message);
                UserRealName = "No User Logged In";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load user data: " + ex.Message);
                UserRealName = "Error Loading User";
            }
        }

        public async Task RefreshHabits()
        {
            await LoadHabits();
        }

        private async Task LoadHabits()
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var formattedDate = _currentDate.Date.ToString("yyyy-MM-dd");
                Debug.WriteLine($"Loading habits for user {userId} on date {formattedDate}");

                var habits = await _habitService.GetHabitsByDateAndUserId(_currentDate, userId);

                Debug.WriteLine($"Retrieved {habits.Count()} habits for user {userId} on date {formattedDate}");

                Habits.Clear();

                foreach (var habit in habits)
                {
                    var currentDayFrequency = Habit.ConvertDayToHabitFrequency(_currentDate.DayOfWeek);
                    Debug.WriteLine($"Habit: {habit.Name}, Frequency: {habit.Frequency}, CurrentDayFrequency: {currentDayFrequency}");

                    // Ensure we only compare dates, ignoring the time component
                    if (habit.StartDate.Date <= _currentDate.Date && habit.Frequency.HasFlag(currentDayFrequency))
                    {
                        Debug.WriteLine($"Adding habit: {habit.Name} for today");
                        Habits.Add(habit);
                    }
                    else
                    {
                        Debug.WriteLine($"Skipping habit: {habit.Name} for today");
                    }

                    Debug.WriteLine($"Loaded {Habits.Count} habits into the view model");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadHabits: {ex.Message}");
            }
        }



        [RelayCommand]
        private async Task OpenUserProfile()
        {
            await Shell.Current.GoToAsync("UserProfilePage");
        }

        private async Task AddProgress(Guid habitId)
        {
            var progress = new HabitProgress
            {
                ProgressId = Guid.NewGuid(),
                Date = CurrentDate,
                CurrentRepetition = 1,
                IsCompleted = false,
                HabitId = habitId
            };

            await _habitService.AddOrUpdateHabitProgress(progress);
            HabitProgresses.Add(progress);
        }

        [RelayCommand]
        public async Task ViewHabit(Habit habit)
        {
            if (habit != null)
            {
                var route = $"UpdateHabitPage?HabitId={habit.UserId}";
                await Shell.Current.GoToAsync(route);
            }
        }

        [RelayCommand]
        public async Task SelectHabit(Habit selectedHabit)
        {
            if (selectedHabit != null)
            {
                var habitJson = JsonConvert.SerializeObject(selectedHabit);
                var route = $"UpdateHabitPage?HabitObject={Uri.EscapeDataString(habitJson)}&SelectedDate={CurrentDate.ToString("o")}";
                await Shell.Current.GoToAsync(route);
            }
        }

        [RelayCommand]
        public async Task ToggleHabitCompleted(Habit habit)
        {
            if (habit != null)
            {
                habit.IsCompleted = !habit.IsCompleted;
                await _habitService.UpdateHabit(habit);
                await LoadHabits();
            }
        }
        private async Task CheckAndResetStreak(Habit habit)
        {
            var lastProgress = await _habitService.GetLastHabitProgress(habit.HabitId);
            if (lastProgress != null)
            {
                var timeSinceLastProgress = CurrentDate - lastProgress.Date;
                if (timeSinceLastProgress.TotalDays > 1)  // Check if more than one day has passed
                {
                    habit.Streak = 0;
                    await _habitService.UpdateHabit(habit);
                    Debug.WriteLine($"Streak reset for habit {habit.Name}.");
                }
            }
        }


        [RelayCommand]
        public async Task IncrementRepetition(Habit habit)
        {
            if (habit == null)
            {
                Debug.WriteLine("IncrementRepetition Error: Habit is null.");
                return;
            }

            try
            {
                var targetDate = CurrentDate.Date;
                Debug.WriteLine($"IncrementRepetition called for habitId: {habit.HabitId} on date: {targetDate}");

                // Fetch existing progress for the current date
                var progress = await _habitService.GetHabitProgress(habit.HabitId, targetDate);

                if (progress == null)
                {
                    // No progress for today, create a new entry
                    Debug.WriteLine("No existing progress found. Creating new progress.");
                    progress = new HabitProgress
                    {
                        ProgressId = Guid.NewGuid(),
                        Date = targetDate,
                        CurrentRepetition = 0,
                        IsCompleted = false,
                        HabitId = habit.HabitId
                    };
                }

                // Ensure that progress cannot exceed the target repetitions for the day
                if (progress.IsCompleted)
                {
                    Debug.WriteLine("Habit is already completed for today. No further increment.");
                    return;
                }

                progress.CurrentRepetition++;
                Debug.WriteLine($"Incremented CurrentRepetition: {progress.CurrentRepetition}");

                if (progress.CurrentRepetition >= habit.TargetRepetition)
                {
                    progress.IsCompleted = true;
                    habit.Streak++;
                    Debug.WriteLine($"Streak incremented to: {habit.Streak}");

                    // Show toast notification for habit completion
                    var toast = Toast.Make($"Congrats! You finished {habit.Name}. \nStreak: {habit.Streak} days", ToastDuration.Long, 14);
                    await toast.Show();
                }

                await _habitService.AddOrUpdateHabitProgress(progress);
                Debug.WriteLine($"Progress saved. CurrentRepetition: {progress.CurrentRepetition}");

                habit.CurrentRepetition = progress.CurrentRepetition;
                habit.IsCompleted = progress.IsCompleted;

                await _habitService.UpdateHabit(habit);

                MessagingCenter.Send(this, "HabitProgressUpdated", habit);

                await LoadHabits();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncrementRepetition Error: {ex.Message}");
            }
        }



        [RelayCommand]
        public async Task DecrementRepetition(Habit habit)
        {
            if (habit == null)
            {
                Debug.WriteLine("DecrementRepetition Error: Habit is null.");
                return;
            }

            try
            {
                var targetDate = CurrentDate.Date;
                Debug.WriteLine($"DecrementRepetition called for habitId: {habit.HabitId} on date: {targetDate}");

                var progress = await _habitService.GetHabitProgress(habit.HabitId, targetDate);
                if (progress != null && progress.CurrentRepetition > 0)
                {
                    progress.CurrentRepetition--;
                    Debug.WriteLine($"Decremented CurrentRepetition to: {progress.CurrentRepetition}");

                    // If the habit is no longer completed, adjust the streak
                    if (progress.IsCompleted && progress.CurrentRepetition < habit.TargetRepetition)
                    {
                        progress.IsCompleted = false;
                        habit.Streak--;
                        Debug.WriteLine($"Streak decremented to: {habit.Streak}");
                    }

                    await _habitService.AddOrUpdateHabitProgress(progress);
                    habit.CurrentRepetition = progress.CurrentRepetition;
                    habit.IsCompleted = progress.IsCompleted;

                    await _habitService.UpdateHabit(habit);
                    await LoadHabits();
                }
                else
                {
                    Debug.WriteLine($"Error: No progress found for habitId: {habit.HabitId} on date: {targetDate} or repetition already at 0");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DecrementRepetition Error: {ex.Message}");
            }
        }


        public void ShowPopup()
        {
            IsPopupVisible = true;
        }

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
            await Shell.Current.GoToAsync("AddHabitPage");
        }

        [RelayCommand]
        private async Task OpenHabitDetail(Habit habit)
        {
            if (habit != null)
            {
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
                await LoadHabits();
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
            MessagingCenter.Unsubscribe<UpdateHabitViewModel>(this, "HabitStreakReset");
        }
    }
}
