using NutritionApp.Views;

namespace NutritionApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
            Routing.RegisterRoute(nameof(HistoryDetailPage), typeof(HistoryDetailPage));
            Routing.RegisterRoute(nameof(WorkoutDetailPage), typeof(WorkoutDetailPage));
            Routing.RegisterRoute(nameof(AddFoodPage), typeof(AddFoodPage));
            Routing.RegisterRoute(nameof(MyRationPage), typeof(MyRationPage));
        }
    }
}