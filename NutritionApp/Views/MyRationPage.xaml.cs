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

        private void OnAddFoodTapped(object sender, TappedEventArgs e)
        {
            if (sender is BindableObject bindable && bindable.BindingContext is MealGroup group)
            {
                var command = _viewModel.AddFoodCommand;
                if (command.CanExecute(group.Name))
                {
                    command.Execute(group.Name);
                }
            }
        }
    }
}
