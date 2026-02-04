using System.Diagnostics;
using NutritionApp.Services;

namespace NutritionApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var username = UsernameEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введіть ім'я ??ористувача та пароль.");
                return;
            }

            var payload = new
            {
                Username = username,
                PasswordHash = password
            };

            var result = await _apiService.LoginUserAsync(payload);
            Debug.WriteLine($"Login result: UserId={result?.UserId}, Message={result?.Message}");

            if (result != null && result.UserId > 0)
            {
                Preferences.Set("UserId", result.UserId);
                await Shell.Current.GoToAsync("//MainApp");
            }
            else
            {
                ShowError(result?.Message ?? "Неправильний логін або пароль.");
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

    private async void OnGoogleSignInClicked(object sender, EventArgs e)
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            await _apiService.OpenGoogleAuthAsync();
        }
        catch (Exception ex)
        {
            ShowError($"Помилка: {ex.Message}");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        string email = await DisplayPromptAsync(
            "Відновлення паролю",
            "Введіть ваш email:",
            "Відправити",
            "Скасувати",
            "email@example.com",
            keyboard: Keyboard.Email);

        if (!string.IsNullOrWhiteSpace(email))
        {
            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                await _apiService.ForgotPasswordAsync(email);

                await DisplayAlert("Готово",
                    "Якщо email існує, ви отримаєте лист з інструкціями.",
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }
    }

    private async void OnGoToRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}