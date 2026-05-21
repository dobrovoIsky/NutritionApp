using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class ProductSelectionPage : ContentPage
{
    private readonly ProductSelectionViewModel _viewModel;
    private readonly Action<List<string>> _onProductsSelected;

    public ProductSelectionPage(List<string> preselectedProducts, Action<List<string>> onProductsSelected)
    {
        InitializeComponent();
        
        _onProductsSelected = onProductsSelected;
        _viewModel = new ProductSelectionViewModel();
        _viewModel.Initialize(preselectedProducts);
        _viewModel.OnProductsSelected = OnProductsConfirmed;
        
        BindingContext = _viewModel;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PopAsync();
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        _viewModel.ConfirmCommand.Execute(null);
        await Shell.Current.Navigation.PopAsync();
    }

    private void OnProductsConfirmed(List<string> selectedProducts)
    {
        _onProductsSelected?.Invoke(selectedProducts);
    }
}
