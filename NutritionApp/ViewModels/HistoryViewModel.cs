using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;
using NutritionApp.Views;

namespace NutritionApp.ViewModels
{
    public class HistoryViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        public ObservableCollection<MealPlan> MealPlans { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Прапорець чи дані вже завантажені
        private bool _isDataLoaded = false;

        public ICommand LoadHistoryCommand { get; }
        public ICommand ViewPlanDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        public HistoryViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadHistoryCommand = new Command(async () => await LoadHistoryAsync(forceRefresh: false));
            RefreshCommand = new Command(async () => await LoadHistoryAsync(forceRefresh: true));
            ViewPlanDetailsCommand = new Command<MealPlan>(async (plan) => await GoToDetails(plan));
        }

        public async Task LoadHistoryAsync(bool forceRefresh = false)
        {
            // Якщо дані вже завантажені і не примусове оновлення - пропускаємо
            if (_isDataLoaded && !forceRefresh && MealPlans.Count > 0)
                return;

            if (IsLoading) return;

            try
            {
                IsLoading = true;
                MealPlans.Clear();

                int userId = Preferences.Get("UserId", 0);
                if (userId > 0)
                {
                    var plans = await _apiService.GetMealPlanHistoryAsync(userId);
                    foreach (var plan in plans)
                    {
                        MealPlans.Add(plan);
                    }
                    _isDataLoaded = true;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Скидання при логауті
        public void Reset()
        {
            _isDataLoaded = false;
            MealPlans.Clear();
        }

        async Task GoToDetails(MealPlan plan)
        {
            if (plan == null) return;
            await Shell.Current.GoToAsync(nameof(HistoryDetailPage), new Dictionary<string, object>
            {
                { "PlanText", plan.Plan }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}