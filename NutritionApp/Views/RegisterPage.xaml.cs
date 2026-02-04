using System.Diagnostics;
using System.Globalization;
using NutritionApp.Services;

namespace NutritionApp.Views;

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
            var email = EmailEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Заповніть ім'я користувача та пароль.");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Пароль повинен містити мінімум 6 символів.");
                return;
            }

            if (password != confirm)
            {
                ShowError("Паролі не співпадають.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            {
                ShowError("Невірний формат email.");
                return;
            }

            var payload = new
            {
                Username = username,
                PasswordHash = password,
                Email = email ?? string.Empty,
                Height = ParseDouble(HeightEntry.Text),
                Weight = ParseDouble(WeightEntry.Text),
                Age = ParseInt(AgeEntry.Text),
                Goal = ConvertGoal(GoalPicker.SelectedItem as string),
                ActivityLevel = ConvertActivity(ActivityPicker.SelectedItem as string),
                Gender = ConvertGender(GenderPicker.SelectedItem as string)
            };

            var result = await _apiService.RegisterUserAsync(payload);

            if (result != null && result.UserId > 0)
            {
                Preferences.Set("UserId", result.UserId);
                await DisplayAlert("Успіх", "Реєстрація успішна!", "OK");
                await Shell.Current.GoToAsync("//MainApp");
            }
            else
            {
                ShowError(result?.Message ?? "Помилка реєстрації.");
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

    private async void OnGoogleSignUpClicked(object sender, EventArgs e)
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

    private async void OnGoToLoginButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }

    private string ConvertGoal(string goal) => goal switch
    {
        "Схуднення" => "lose weight",
        "Набір маси" => "gain muscle",
        "Підтримка ваги" => "maintain weight",
        _ => "maintain weight"
    };

    private string ConvertActivity(string activity) => activity switch
    {
        "Сидячий спосіб життя" => "sedentary",
        "Легка активність" => "lightly active",
        "Помірна активність" => "moderately active",
        "Висока активність" => "very active",
        "Дуже висока активність" => "extra active",
        _ => "moderately active"
    };

    private string ConvertGender(string gender) => gender switch
    {
        "Чоловіча" => "male",
        "Жіноча" => "female",
        _ => "male"
    };

    private double ParseDouble(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v);
        return v;
    }

    private int ParseInt(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        int.TryParse(s, out var v);
        return v;
    }
}