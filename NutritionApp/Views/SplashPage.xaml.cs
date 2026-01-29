namespace NutritionApp.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var anim1 = Logo.FadeTo(1, 600, Easing.CubicOut);
        var anim2 = Logo.ScaleTo(1.05, 700, Easing.CubicOut);
        var anim3 = TitleLabel.FadeTo(1, 500, Easing.CubicOut);

        await Task.WhenAll(anim1, anim2, anim3);
        await Task.Delay(400);

        Application.Current.MainPage = new AppShell();
    }
}