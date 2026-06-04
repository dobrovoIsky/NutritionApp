using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await Task.WhenAll(
                _viewModel.StartPedometerAsync(),
                _viewModel.LoadUserProfileAsync());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainPage.OnAppearing error: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopPedometer();
    }
}