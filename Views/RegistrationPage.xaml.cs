using HabitTracker.Services;
using HabitTracker.ViewModels;

namespace HabitTracker.Views;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage(AddUserViewModel addUserViewModel)
	{
		InitializeComponent();
		BindingContext = addUserViewModel;

	}
}