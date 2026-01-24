
using System.Globalization;
using NutritionApp.Services; // Додайте цей using
using Microsoft.Maui.Controls;


namespace NutritionApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        MainPage = new AppShell();

        // Встановлюємо світлу тему при першому запуску
        
    }
}