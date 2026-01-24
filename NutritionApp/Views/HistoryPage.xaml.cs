using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;
    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Завантажуємо історію щоразу, коли сторінка стає видимою
        _viewModel.LoadHistoryCommand.Execute(null);
    }
}