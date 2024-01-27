using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class HabitsListingPage : ContentPage
{
    public HabitsListingPage(HabitsListingViewModel habitsListingViewModel)
    {
        InitializeComponent();
        BindingContext = habitsListingViewModel;
    }
}