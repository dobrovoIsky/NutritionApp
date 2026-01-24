using NutritionApp.ViewModels;

namespace NutritionApp.Views;

public partial class MealPlanPage : ContentPage
{
    public MealPlanPage(MealPlanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}