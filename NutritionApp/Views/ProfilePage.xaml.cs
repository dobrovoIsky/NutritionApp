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
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }
}