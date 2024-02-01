namespace HabitTracker.Views;

public partial class CustomPopupPage : ContentPage
{
    public CustomPopupPage()
    {
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Start with a fully transparent popup
        this.Opacity = 0;

        // Animate to full opacity over 500 milliseconds
        await this.FadeTo(1, 500);
    }

}