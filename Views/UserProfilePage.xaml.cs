using HabitTracker.ViewModels;
using Microsoft.Maui.Controls;

namespace HabitTracker.Views
{
    public partial class UserProfilePage : ContentPage
    {
        public UserProfilePage(UserProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ((UserProfileViewModel)BindingContext).LoadUserAsync();
        }
    }
}
