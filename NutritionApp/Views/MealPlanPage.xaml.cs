using System.Diagnostics;
using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class MealPlanPage : ContentPage
{
    private readonly MealPlanViewModel _viewModel;

    public MealPlanPage(MealPlanViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnGenerateClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("MealPlanPage: Generate button CLICKED!");
        // Команда вже прив'язана через Command binding, 
        // але якщо вона не спрацювала — виконаємо напряму
        if (_viewModel.GeneratePlanCommand.CanExecute(null))
        {
            _viewModel.GeneratePlanCommand.Execute(null);
        }
    }
}