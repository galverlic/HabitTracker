using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using System.Threading.Tasks;

namespace HabitTracker.ViewModels
{
    public partial class AddHabitViewModel : ObservableObject
    {
        private readonly IHabitService _habitService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private string habitName;
        [ObservableProperty]
        private string habitDescription;

        [ObservableProperty]
        private int? habitTargetRepetition;

        [ObservableProperty]
        private bool habitIsCompleted;

        // Checkbox properties
        [ObservableProperty]
        private bool isMondaySelected;
        [ObservableProperty]
        private bool isTuesdaySelected;
        [ObservableProperty]
        private bool isWednesdaySelected;
        [ObservableProperty]
        private bool isThursdaySelected;
        [ObservableProperty]
        private bool isFridaySelected;
        [ObservableProperty]
        private bool isSaturdaySelected;
        [ObservableProperty]
        private bool isSundaySelected;

        public AddHabitViewModel(IHabitService habitService, IUserService userService)
        {
            _habitService = habitService;
            _userService = userService;
        }

        private Habit.HabitFrequency CalculateFrequency()
        {
            Habit.HabitFrequency frequency = Habit.HabitFrequency.None;

            if (IsMondaySelected) frequency |= Habit.HabitFrequency.Monday;
            if (IsTuesdaySelected) frequency |= Habit.HabitFrequency.Tuesday;
            if (IsWednesdaySelected) frequency |= Habit.HabitFrequency.Wednesday;
            if (IsThursdaySelected) frequency |= Habit.HabitFrequency.Thursday;
            if (IsFridaySelected) frequency |= Habit.HabitFrequency.Friday;
            if (IsSaturdaySelected) frequency |= Habit.HabitFrequency.Saturday;
            if (IsSundaySelected) frequency |= Habit.HabitFrequency.Sunday;

            return frequency;
        }

        [RelayCommand]
        private async Task AddHabit()
        {
            var userId = await _userService.GetCurrentUserId();
            var frequency = GetHabitFrequency();
            var newHabit = new Habit
            {
                HabitId = Guid.NewGuid(),
                Name = HabitName,
                Description = HabitDescription,
                Frequency = frequency,
                CurrentRepetition = 0,
                TargetRepetition = HabitTargetRepetition,
                StartDate = DateTime.Now.Date,  // Set StartDate to current date
                ReminderTime = null,
                IsCompleted = false,
                Streak = 0,
                UserId = userId
            };

            await _habitService.CreateHabit(newHabit);
            MessagingCenter.Send(this, "HabitAdded");
            await Shell.Current.GoToAsync("..");
        }

        private Habit.HabitFrequency GetHabitFrequency()
        {
            var frequency = Habit.HabitFrequency.None;
            if (IsMondaySelected) frequency |= Habit.HabitFrequency.Monday;
            if (IsTuesdaySelected) frequency |= Habit.HabitFrequency.Tuesday;
            if (IsWednesdaySelected) frequency |= Habit.HabitFrequency.Wednesday;
            if (IsThursdaySelected) frequency |= Habit.HabitFrequency.Thursday;
            if (IsFridaySelected) frequency |= Habit.HabitFrequency.Friday;
            if (IsSaturdaySelected) frequency |= Habit.HabitFrequency.Saturday;
            if (IsSundaySelected) frequency |= Habit.HabitFrequency.Sunday;
            return frequency;
        }
    
    private Habit.HabitFrequency ConvertDayToHabitFrequency(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => Habit.HabitFrequency.Monday,
                DayOfWeek.Tuesday => Habit.HabitFrequency.Tuesday,
                DayOfWeek.Wednesday => Habit.HabitFrequency.Wednesday,
                DayOfWeek.Thursday => Habit.HabitFrequency.Thursday,
                DayOfWeek.Friday => Habit.HabitFrequency.Friday,
                DayOfWeek.Saturday => Habit.HabitFrequency.Saturday,
                DayOfWeek.Sunday => Habit.HabitFrequency.Sunday,
                _ => Habit.HabitFrequency.None,
            };
        }
    }
}
