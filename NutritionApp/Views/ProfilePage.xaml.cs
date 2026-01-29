using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Перевірка теми
        if (Application.Current.Resources.TryGetValue("PageBackgroundColor", out var colorValue) && colorValue is Color color)
        {
            ThemeSwitch.IsToggled = color != Colors.White;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadUserProfileAsync();

        // Оновлюємо ініціали аватара та інші значення після завантаження профілю
        UpdateProfileDisplay();
    }

    private void UpdateProfileDisplay()
    {
        if (_viewModel.UserProfile != null)
        {
            // Ініціали: перші літери username (якщо username є, наприклад, "First Last" -> "FL")
            var username = _viewModel.UserProfile.Username;
            if (!string.IsNullOrEmpty(username))
            {
                var parts = username.Split(' ');
                var initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                    : username.Length > 0 ? username[0].ToString().ToUpper() : "U";
                AvatarInitials.Text = initials;
            }

            // Заповнюємо значення (якщо bindings не працюють для всіх полів)
            HeightValue.Text = _viewModel.UserProfile.Height.ToString();
            WeightValue.Text = _viewModel.UserProfile.Weight.ToString();
            AgeValue.Text = _viewModel.UserProfile.Age.ToString();
            ActivityValue.Text = _viewModel.UserProfile.ActivityLevel;
            GoalValue.Text = _viewModel.UserProfile.Goal;
        }
    }

    private void OnThemeToggled(object sender, ToggledEventArgs e)
    {
        Color customGray = Color.FromHex("#EAEAEA");

        if (e.Value)
        {
            Application.Current.Resources["PageBackgroundColor"] = customGray;
        }
        else
        {
            Application.Current.Resources["PageBackgroundColor"] = Colors.White;
        }
    }

    private async void OnLogoutButtonClicked(object sender, EventArgs e)
    {
        Preferences.Clear();
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        if (_viewModel.UserProfile != null)
        {
            await Shell.Current.GoToAsync(nameof(EditProfilePage), new Dictionary<string, object>
            {
                { "UserProfile", _viewModel.UserProfile }
            });
        }
    }
}