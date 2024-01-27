using CommunityToolkit.Maui;
using HabitTracker.Models;
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

            // Configure Supabase
            var url = AppConfig.SUPABASE_URL;
            var key = AppConfig.SUPABASE_KEY;
            builder.Services.AddSingleton(provider => new Supabase.Client(url, key));

            // Add ViewModels
            builder.Services.AddSingleton<HabitsListingViewModel>();
            builder.Services.AddTransient<AddHabitViewModel>();
            builder.Services.AddTransient<UpdateHabitViewModel>();

            // Add Views
            builder.Services.AddSingleton<HabitsListingPage>();
            builder.Services.AddTransient<AddHabitPage>();
            builder.Services.AddTransient<UpdateHabitPage>();

            // Add Data Service
            builder.Services.AddSingleton<IDataService, DataService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
