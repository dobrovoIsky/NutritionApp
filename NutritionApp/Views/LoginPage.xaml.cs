using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using NutritionApp.Services;

namespace NutritionApp.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;

        public LoginPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        // Обробник натискання кнопки логіну
        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var username = UsernameEntry?.Text?.Trim();
                var password = PasswordEntry?.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    await DisplayAlert("Помилка", "Введіть логін і пароль.", "OK");
                    return;
                }

                var payload = new { username = username, password = password };
                var result = await _apiService.LoginUserAsync(payload);

                Debug.WriteLine($"Login result: {System.Text.Json.JsonSerializer.Serialize(result)}");

                if (result != null && result.UserId > 0)
                {
                    // Зберігаємо UserId у Preferences як int
                    Preferences.Set("UserId", result.UserId);
                    Debug.WriteLine($"Saved UserId = {result.UserId} into Preferences");

                    // Перехід до головної сторінки (очищує стек)
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                }
                else
                {
                    await DisplayAlert("Помилка", result?.Message ?? "Не вдалося увійти.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login exception: {ex.Message}");
                await DisplayAlert("Помилка", $"Помилка під час логіну: {ex.Message}", "OK");
            }
        }

        // Обробник - забули пароль
        private async void OnForgotPasswordClicked(object sender, EventArgs e)
        {
            // Тимчасова поведінка: показати повідомлення
            await DisplayAlert("Відновлення пароля", "Інструкції щодо відновлення пароля будуть надіслані на вашу електронну пошту.", "OK");

            // Якщо є окрема сторінка відновлення - навігація:
            // await Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
        }

        // Обробник переходу на сторінку реєстрації
        private async void OnGoToRegisterButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Якщо маршрут зареєстрований в AppShell, використовуємо його ім'я
                await Shell.Current.GoToAsync(nameof(RegisterPage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation to RegisterPage failed: {ex.Message}");
                // Запасний варіант: модальна навігація або Push
                try
                {
                    await Navigation.PushAsync(new RegisterPage(_apiService));
                }
                catch
                {
                    // якщо й це не працює — нічого не робимо
                }
            }
        }
    }
}