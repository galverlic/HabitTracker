using HabitTracker.Models;
using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class HabitsListingPage : ContentPage
{
    public HabitsListingPage(HabitsListingViewModel habitsListingViewModel)
    {
        InitializeComponent();
        BindingContext = habitsListingViewModel;


    }

    private async void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is Habit habit)
        {
            var viewModel = (HabitsListingViewModel)BindingContext;
            await viewModel.UpdateHabitCompletionStatus(habit, e.Value);
        }
    }


}
