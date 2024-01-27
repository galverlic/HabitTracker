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
            // Call a method in the ViewModel to handle the update
            var viewModel = (HabitsListingViewModel)BindingContext;
            await viewModel.UpdateHabitCompletionStatus(habit);
        }
    }

    // In HabitsListingPage.xaml.cs
    //private void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
    //{
    //    if (sender is CheckBox checkBox && checkBox.BindingContext is Habit habit)
    //    {
    //        ((HabitsListingViewModel)BindingContext).ToggleHabitCompletionCommand.Execute(habit);
    //    }
    //}





}