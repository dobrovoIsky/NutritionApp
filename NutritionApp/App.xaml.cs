using NutritionApp.Views;

namespace NutritionApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Підписуємось на Google Auth callback
        MessagingCenter.Subscribe<object, string>(this, "GoogleAuthCallback", async (sender, uri) =>
        {
            await HandleGoogleAuthCallback(uri);
        });

        MainPage = new SplashPage();
    }

    private async Task HandleGoogleAuthCallback(string uri)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Handling Google callback: {uri}");

            // Парсимо URI: nutritionapp://auth/callback?token=xxx&userId=123
            var uriObj = new Uri(uri);
            var query = System.Web.HttpUtility.ParseQueryString(uriObj.Query);

            var token = query["token"];
            var userIdStr = query["userId"];

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                // Зберігаємо userId
                Preferences.Set("UserId", userId);
                Preferences.Set("AuthToken", token ?? "");

                System.Diagnostics.Debug.WriteLine($"Google auth successful! UserId: {userId}");

                // Переходимо на головну сторінку
                await Shell.Current.GoToAsync("//MainApp");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling Google callback: {ex.Message}");
        }
    }
}