using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class EquipmentSelectionPage : ContentPage
{
    private readonly EquipmentSelectionViewModel _viewModel;
    private readonly Action<List<string>> _onEquipmentSelected;

    public EquipmentSelectionPage(List<string> preselectedEquipment, Action<List<string>> onEquipmentSelected)
    {
        InitializeComponent();
        
        _onEquipmentSelected = onEquipmentSelected;
        _viewModel = new EquipmentSelectionViewModel();
        _viewModel.Initialize(preselectedEquipment);
        _viewModel.OnEquipmentSelected = OnEquipmentConfirmed;
        
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

    private void OnEquipmentConfirmed(List<string> selectedEquipment)
    {
        _onEquipmentSelected?.Invoke(selectedEquipment);
    }
}
