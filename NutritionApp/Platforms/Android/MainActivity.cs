using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace NutritionApp;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "nutritionapp",
    DataHost = "auth",
    DataPathPrefix = "/callback")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        HandleIntent(Intent);
    }

    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);
        HandleIntent(intent);
    }

    private void HandleIntent(Intent intent)
    {
        if (intent?.Data != null)
        {
            var uri = intent.Data.ToString();
            System.Diagnostics.Debug.WriteLine($"Deep link received: {uri}");

            // Відправляємо URI в наш сервіс
            if (uri.StartsWith("nutritionapp://auth/callback"))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send<object, string>(this, "GoogleAuthCallback", uri);
                });
            }
        }
    }
}