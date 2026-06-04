using Microsoft.Maui.Controls;
using NutritionApp.ViewModels;

namespace NutritionApp.Views
{
    public partial class LeaderboardPage : ContentPage
    {
        private LeaderboardViewModel _viewModel;

        public LeaderboardPage()
        {
            InitializeComponent();
            _viewModel = new LeaderboardViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadLeaderboardCommand.Execute(null);
        }
    }
}
