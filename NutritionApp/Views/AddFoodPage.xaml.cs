using NutritionApp.ViewModels;

namespace NutritionApp.Views
{
    public partial class AddFoodPage : ContentPage, IQueryAttributable
    {
        private readonly AddFoodViewModel _viewModel;

        public AddFoodPage(AddFoodViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("mealType"))
            {
                var val = query["mealType"]?.ToString();
                if (!string.IsNullOrEmpty(val))
                {
                    _viewModel.MealType = Uri.UnescapeDataString(val);
                }
            }

            if (query.ContainsKey("foodEntry") && query["foodEntry"] is Models.FoodEntry entry)
            {
                _viewModel.InitializeForEdit(entry);
            }
        }
    }
}
