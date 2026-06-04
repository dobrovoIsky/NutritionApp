using Microsoft.Maui.Controls;
using NutritionApp.ViewModels;
using System.Diagnostics;

namespace NutritionApp.Views
{
    public partial class LeaderboardPage : ContentPage
    {
        private readonly LeaderboardViewModel _viewModel;

        public LeaderboardPage(LeaderboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                _viewModel.LoadLeaderboardCommand.Execute(null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LeaderboardPage.OnAppearing error: {ex.Message}");
            }
        }
    }
}
