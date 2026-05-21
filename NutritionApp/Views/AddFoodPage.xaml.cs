using NutritionApp.ViewModels;

namespace NutritionApp.Views
{
    public partial class AddFoodPage : ContentPage
    {
        private readonly AddFoodViewModel _viewModel;

        public AddFoodPage(AddFoodViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}
