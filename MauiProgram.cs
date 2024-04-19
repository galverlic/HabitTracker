using CommunityToolkit.Maui;
using HabitTracker.Data;
using HabitTracker.Services;
using HabitTracker.ViewModels;
using HabitTracker.Views;
using Microsoft.Extensions.Logging;

namespace HabitTracker

{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            ;


            var url = AppConfig.SUPABASE_URL;
            var key = AppConfig.SUPABASE_KEY;
            builder.Services.AddSingleton(provider => new Supabase.Client(url, key));



            // Note the creation as a singleton.
            // Add ViewModels
            builder.Services.AddSingleton<HabitsListingViewModel>();
            builder.Services.AddTransient<AddHabitViewModel>();
            builder.Services.AddTransient<UpdateHabitViewModel>();
            builder.Services.AddTransient<AddUserViewModel>();
            builder.Services.AddTransient<LoginViewModel>();


            // Add Views
            builder.Services.AddSingleton<HabitsListingPage>();
            builder.Services.AddTransient<AddHabitPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<UpdateHabitPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddSingleton<UserProfilePage>();
            // Add Data Service
            builder.Services.AddSingleton<IHabitService, HabitService>();
            builder.Services.AddSingleton<IUserService, UserService>();



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
