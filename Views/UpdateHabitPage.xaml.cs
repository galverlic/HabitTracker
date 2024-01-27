using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class UpdateHabitPage : ContentPage
{
    public UpdateHabitPage(UpdateHabitViewModel updateHabitViewModel)
    {
        InitializeComponent();
        BindingContext = updateHabitViewModel;
    }
}