using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class UpdateHabitPage : ContentPage
{
    public UpdateHabitPage(UpdateHabitViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
    }
}