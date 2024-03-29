using HabitTracker.Views;

namespace HabitTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            RegisterForRoute<AddHabitPage>();
            RegisterForRoute<UpdateHabitPage>();
            RegisterForRoute<HabitsListingPage>();

        }

        protected void RegisterForRoute<T>()
        {
            Routing.RegisterRoute(typeof(T).Name, typeof(T));
        }
    }
}
