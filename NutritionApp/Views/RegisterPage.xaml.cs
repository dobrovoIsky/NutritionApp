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
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text ?? string.Empty;
        var confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;
        var heightText = HeightEntry.Text ?? string.Empty;
        var weightText = WeightEntry.Text ?? string.Empty;
        var ageText = AgeEntry.Text ?? string.Empty;
        var goal = GoalPicker.SelectedItem?.ToString() ?? "maintain weight";
        var activity = ActivityPicker.SelectedItem?.ToString() ?? "sedentary";
        var gender = GenderPicker.SelectedItem?.ToString() ?? "male";

        // Перевірка полів
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
            !double.TryParse(heightText, out var height) || height <= 0 ||
            !double.TryParse(weightText, out var weight) || weight <= 0 ||
            !int.TryParse(ageText, out var age) || age <= 0)
        {
            ErrorLabel.Text = "Будь ласка, заповніть усі поля коректно.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (password != confirmPassword)
        {
            ErrorLabel.Text = "Паролі не співпадають.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (password.Length < 8)
        {
            ErrorLabel.Text = "Пароль має бути щонайменше 8 символів.";
            ErrorLabel.IsVisible = true;
            return;
        }

        ErrorLabel.Text = string.Empty;
        ErrorLabel.IsVisible = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var payload = new
            {
                Username = username,
                PasswordHash = password, // Надсилаємо простий пароль
                Height = height,
                Weight = weight,
                Age = age,
                Goal = goal,
                ActivityLevel = activity,
                Gender = gender
            };

            var response = await _apiService.RegisterUserAsync(payload);

            if (response != null && response.UserId > 0)
            {
                await DisplayAlert("Успіх", "Реєстрація успішна! Тепер увійдіть.", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                ErrorLabel.Text = response?.Message ?? "Помилка реєстрації. Спробуйте ще раз.";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Помилка з'єднання: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnGoToLoginButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}