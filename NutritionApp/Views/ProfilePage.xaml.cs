namespace NutritionApp.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        // Можна залишити цей рядок, щоб перемикач відповідав поточному стану
        if (Application.Current.Resources.TryGetValue("PageBackgroundColor", out var colorValue) && colorValue is Color color)
        {
            ThemeSwitch.IsToggled = color != Colors.White;
        }
    }

    private void OnThemeToggled(object sender, ToggledEventArgs e)
    {
        // Колір, на який будемо змінювати фон (не темно-сірий)
        Color customGray = Color.FromHex("#EAEAEA");

        if (e.Value)
        {
            // Якщо перемикач ВКЛЮЧЕНИЙ - ставимо сірий фон
            Application.Current.Resources["PageBackgroundColor"] = customGray;
        }
        else
        {
            // Якщо ВИМКНЕНИЙ - повертаємо білий фон
            Application.Current.Resources["PageBackgroundColor"] = Colors.White;
        }
    }

    private async void OnLogoutButtonClicked(object sender, EventArgs e)
    {
        Preferences.Clear();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}