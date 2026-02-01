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

                double? height = ParseNullableDouble(HeightEntry.Text);
                double? weight = ParseNullableDouble(WeightEntry.Text);
                int? age = ParseNullableInt(AgeEntry.Text);

                var goal = GoalPicker.SelectedItem as string;
                var activity = ActivityPicker.SelectedItem as string;
                var gender = GenderPicker.SelectedItem as string;

                var payload = new
                {
                    username = username,
                    password = password,
                    height = height,
                    weight = weight,
                    age = age,
                    goal = goal,
                    activityLevel = activity,
                    gender = gender
                };

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

        private double? ParseNullableDouble(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v)) return v;
            if (double.TryParse(s, out v)) return v;
            return null;
        }

        private int? ParseNullableInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (int.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v)) return v;
            if (int.TryParse(s, out v)) return v;
            return null;
        }
    }
}