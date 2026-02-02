using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using NutritionApp.Services;

namespace NutritionApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly ApiService _apiService;

        public RegisterPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                var username = UsernameEntry.Text?.Trim();
                var password = PasswordEntry.Text;
                var confirm = ConfirmPasswordEntry.Text;

                if (string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Будь ласка, заповніть ім'я користувача та пароль.");
                    return;
                }

                if (password != confirm)
                {
                    ShowError("Паролі не співпадають.");
                    return;
                }

                double height = ParseDouble(HeightEntry.Text);
                double weight = ParseDouble(WeightEntry.Text);
                int age = ParseInt(AgeEntry.Text);

                var goal = GoalPicker.SelectedItem as string ?? "maintain weight";
                var activity = ActivityPicker.SelectedItem as string ?? "moderately active";
                var gender = GenderPicker.SelectedItem as string ?? "male";

                // ВИПРАВЛЕНО: Імена полів відповідають UserRegDTO на сервері
                var payload = new
                {
                    Username = username,
                    PasswordHash = password,  // ? Змінено з "password" на "PasswordHash"
                    Height = height,
                    Weight = weight,
                    Age = age,
                    Goal = goal,
                    ActivityLevel = activity,
                    Gender = gender
                };

                Debug.WriteLine($"Register payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");

                var result = await _apiService.RegisterUserAsync(payload);
                Debug.WriteLine($"Register result: {System.Text.Json.JsonSerializer.Serialize(result)}");

                if (result != null && result.UserId > 0)
                {
                    Preferences.Set("UserId", result.UserId);
                    Debug.WriteLine($"Saved UserId = {result.UserId} into Preferences (register)");

                    await DisplayAlert("Успіх", "Реєстрація пройшла успішно.", "OK");
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                }
                else
                {
                    ShowError(result?.Message ?? "Невідома помилка під час реєстрації.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Помилка: {ex.Message}");
            }
            finally
            {
                LoadingIndicator.IsRunning = LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnGoToLoginButtonClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch
            {
                try { await Shell.Current.GoToAsync(nameof(LoginPage)); } catch { await Navigation.PopAsync(); }
            }
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }

        private double ParseDouble(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            if (double.TryParse(s, out v)) return v;
            return 0;
        }

        private int ParseInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            if (int.TryParse(s, out v)) return v;
            return 0;
        }
    }
}