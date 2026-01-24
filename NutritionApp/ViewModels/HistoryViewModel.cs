using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        public ICommand LoadHistoryCommand { get; }
        public ICommand ViewPlanDetailsCommand { get; }

        public HistoryViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadHistoryCommand = new Command(async () => await LoadHistoryAsync());
            ViewPlanDetailsCommand = new Command<MealPlan>(async (plan) => await GoToDetails(plan));
        }

        async Task LoadHistoryAsync()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                MealPlans.Clear();
                int userId = Preferences.Get("userId", 0);
                if (userId > 0)
                {
                    var plans = await _apiService.GetMealPlanHistoryAsync(userId);
                    foreach (var plan in plans)
                    {
                        MealPlans.Add(plan);
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        async Task GoToDetails(MealPlan plan)
        {
            if (plan == null) return;
            // Передаємо текст плану на сторінку деталей
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