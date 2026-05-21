using NutritionApp.ViewModels;

namespace NutritionApp.Views
{
    public partial class MyRationPage : ContentPage
    {
        private readonly MyRationViewModel _viewModel;

        public MyRationPage(MyRationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDataAsync();
        }
    }
}
