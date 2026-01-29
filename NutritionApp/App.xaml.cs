
using System.Globalization;
using NutritionApp.Services; // Додайте цей using
using Microsoft.Maui.Controls;
using NutritionApp.Views;


namespace NutritionApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new SplashPage(); // стартуємо з анімованого екрану

        // Встановлюємо світлу тему при першому запуску

    }
}