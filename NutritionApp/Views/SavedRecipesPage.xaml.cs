using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class SavedRecipesPage : ContentPage
{
    private readonly SavedRecipesViewModel _viewModel;

    public SavedRecipesPage(SavedRecipesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadRecipes();
    }
}
