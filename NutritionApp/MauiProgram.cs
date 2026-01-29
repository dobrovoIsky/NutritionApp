using Microsoft.Extensions.Logging;
using NutritionApp.Services;
using NutritionApp.ViewModels;
using NutritionApp.Views;
using Microsoft.Maui.Controls;

namespace NutritionApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentIcons");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Сервіси й сторінки
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddSingleton<ApiService>();

            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<RegisterPage>();   // стало Singleton
            builder.Services.AddSingleton<MainPage>();

            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddTransient<EditProfilePage>();
            builder.Services.AddTransient<EditProfileViewModel>();

            builder.Services.AddTransient<MealPlanPage>();
            builder.Services.AddTransient<MealPlanViewModel>();

            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<HistoryPage>();
            builder.Services.AddTransient<HistoryViewModel>();
            builder.Services.AddTransient<HistoryDetailPage>();

            return builder.Build();
        }
    }
}