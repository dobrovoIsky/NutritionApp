using Microsoft.Extensions.Logging;
using NutritionApp.Services;
using NutritionApp.ViewModels;
using NutritionApp.Views;
using Plugin.LocalNotification;

using ZXing.Net.Maui.Controls;

namespace NutritionApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentIcons");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Сервіси
            builder.Services.AddSingleton<WaterReminderService>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddSingleton<CacheService>();
            builder.Services.AddSingleton<ApiService>();
#if ANDROID
            builder.Services.AddSingleton<IPedometerService, Platforms.Android.PedometerService>();
#else
            builder.Services.AddSingleton<IPedometerService, PedometerServiceStub>();
#endif

            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<RegisterPage>();
            builder.Services.AddSingleton<MainPage>();

            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddTransient<EditProfilePage>();
            builder.Services.AddTransient<EditProfileViewModel>();

            builder.Services.AddTransient<MealPlanPage>();
            builder.Services.AddSingleton<MealPlanViewModel>(); // Singleton for caching

            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<SavedRecipesPage>();
            builder.Services.AddSingleton<SavedRecipesViewModel>();
            builder.Services.AddTransient<HistoryDetailPage>();
            builder.Services.AddTransient<WorkoutPage>();
            builder.Services.AddTransient<AddFoodPage>();
            builder.Services.AddTransient<AddFoodViewModel>();
            builder.Services.AddTransient<BarcodeScannerPage>();
            builder.Services.AddTransient<HistoryDetailPage>();
            builder.Services.AddTransient<MyRationPage>();
            builder.Services.AddTransient<MyRationViewModel>();

            return builder.Build();
        }
    }
}