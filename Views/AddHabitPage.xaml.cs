using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class AddHabitPage : ContentPage
{
    public AddHabitPage(AddHabitViewModel addHabitViewModel)
    {
        InitializeComponent();
        BindingContext = addHabitViewModel;
    }
}