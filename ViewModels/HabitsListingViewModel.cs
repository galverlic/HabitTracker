using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HabitTracker.ViewModels
{
    public partial class HabitsListingViewModel : ObservableObject
    {
        private readonly IHabitService _habitService;
        private readonly IUserService _userService;
        private readonly UserProfileViewModel _userProfileViewModel;

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
                CurrentDateDisplay = _currentDate.ToString("D"); // Update CurrentDateDisplay
                LoadHabitsCommand.Execute(null);
            }
        }
        [ObservableProperty]
        private bool isPopupVisible;

        public ICommand LoadHabitsCommand { get; }
        public ICommand AddProgressCommand { get; }
        public ICommand GoToPreviousDayCommand { get; }
        public ICommand GoToNextDayCommand { get; }

        [ObservableProperty]
        private string currentDateDisplay;

        public HabitsListingViewModel(IHabitService habitService, IUserService userService)
        {
            _habitService = habitService;
            _userService = userService;

            // Initialize CurrentDate to today's date
            _currentDate = DateTime.Now;
            CurrentDateDisplay = _currentDate.ToString("D");
            LoadHabitsCommand = new Command(async () => await LoadHabits());
            AddProgressCommand = new Command<Guid>(async (habitId) => await AddProgress(habitId));
            GoToPreviousDayCommand = new Command(() => CurrentDate = CurrentDate.AddDays(-1));
            GoToNextDayCommand = new Command(() => CurrentDate = CurrentDate.AddDays(1));
            ClosePopupCommand = new Command(() => IsPopupVisible = false);
        }

        public async Task InitializeUser()
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(userId);
                UserRealName = user.Name;
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

        private async Task LoadHabits()
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var habits = await _habitService.GetHabitsByDateAndUserId(_currentDate, userId);
                Debug.WriteLine($"Retrieved {habits.Count()} habits for user {userId}");
                Habits.Clear();
                foreach (var habit in habits)
                {
                    if (habit.Frequency.HasFlag(Habit.ConvertDayToHabitFrequency(_currentDate.DayOfWeek)))
                    {
                        Debug.WriteLine($"Habit: {habit.Name}, ID: {habit.HabitId}");
                        Habits.Add(habit);
                    }
                }
                Debug.WriteLine($"Loaded {Habits.Count} habits into the view model");
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
            try
            {
                if (selectedHabit != null)
                {
                    var navigationParameter = new Dictionary<string, object>
            {
                { "HabitObject", selectedHabit }
            };
                    await Shell.Current.GoToAsync($"UpdateHabitPage", navigationParameter);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SelectHabit Error: {ex.InnerException?.Message ?? ex.Message}");
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
        [RelayCommand]
        public async Task IncrementRepetition(Habit habit)
        {
            try
            {
                var progress = await _habitService.GetHabitProgress(habit.HabitId, CurrentDate);
                if (progress == null)
                {
                    progress = new HabitProgress
                    {
                        ProgressId = Guid.NewGuid(),
                        Date = CurrentDate,
                        CurrentRepetition = 0,
                        IsCompleted = false,
                        HabitId = habit.HabitId
                    };
                }

                progress.CurrentRepetition++;
                progress.IsCompleted = progress.CurrentRepetition >= habit.TargetRepetition;

                habit.CurrentRepetition = progress.CurrentRepetition;
                habit.IsCompleted = progress.IsCompleted;

                await _habitService.AddOrUpdateHabitProgress(progress);
                await _habitService.UpdateHabit(habit);
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
            try
            {
                var progress = await _habitService.GetHabitProgress(habit.HabitId, CurrentDate);
                if (progress != null)
                {
                    if (progress.CurrentRepetition <= 0)
                    {
                        progress.CurrentRepetition = 0;
                    }
                    else
                    {
                        progress.CurrentRepetition--;
                    }

                    progress.IsCompleted = progress.CurrentRepetition >= habit.TargetRepetition;

                    habit.CurrentRepetition = progress.CurrentRepetition;
                    habit.IsCompleted = progress.IsCompleted;

                    await _habitService.AddOrUpdateHabitProgress(progress);
                    await _habitService.UpdateHabit(habit);
                    await LoadHabits();
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
