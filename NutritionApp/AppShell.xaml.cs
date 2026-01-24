using NutritionApp.Views;

namespace NutritionApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
            Routing.RegisterRoute(nameof(MealPlanPage), typeof(MealPlanPage));
            Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
            Routing.RegisterRoute(nameof(HistoryDetailPage), typeof(HistoryDetailPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage)); // Додайте цей рядок
        }
    }
}