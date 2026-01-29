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

    async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorLabel.Text = "Будь ласка, заповніть усі поля.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (password.Length < 8)
        {
            ErrorLabel.Text = "Пароль має містити щонайменше 8 символів.";
            ErrorLabel.IsVisible = true;
            return;
        }

        ErrorLabel.Text = string.Empty;
        ErrorLabel.IsVisible = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var response = await _apiService.LoginUserAsync(new { username, passwordHash = password });

            if (response != null && response.UserId > 0)
            {
                Preferences.Set("userId", response.UserId);
                await Shell.Current.GoToAsync("//MainApp");
            }
            else
            {
                ErrorLabel.Text = "Неправильний логін або пароль.";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Не вдалося підключитися до сервера: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        // TODO: навігація на відновлення, якщо буде
    }

    async void OnGoToRegisterButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (SignUpButton is not null)
                SignUpButton.IsEnabled = false;

            // Швидка навігація без анімації, щоб не зависало
            await Shell.Current.GoToAsync(nameof(RegisterPage), animate: false);
        }
        finally
        {
            if (SignUpButton is not null)
                SignUpButton.IsEnabled = true;
        }
    }
}