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
        public ObservableCollection<WorkoutPlan> Workouts { get; } = new();

        private bool _isMealPlanTab = true;
        public bool IsMealPlanTab
        {
            get => _isMealPlanTab;
            set { _isMealPlanTab = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsWorkoutTab)); }
        }
        public bool IsWorkoutTab => !_isMealPlanTab;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Прапорець чи дані вже завантажені
        private bool _isDataLoaded = false;

        private bool _showFavoritesOnly;
        public bool ShowFavoritesOnly
        {
            get => _showFavoritesOnly;
            set
            {
                _showFavoritesOnly = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        public ObservableCollection<MealPlan> FilteredMealPlans { get; } = new();

        public ICommand LoadHistoryCommand { get; }
        public ICommand ViewPlanDetailsCommand { get; }
        public ICommand ViewWorkoutDetailsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SwitchTabCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }

        public HistoryViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadHistoryCommand = new Command(async () => await LoadHistoryAsync(forceRefresh: false));
            RefreshCommand = new Command(async () => await LoadHistoryAsync(forceRefresh: true));
            ViewPlanDetailsCommand = new Command<MealPlan>(async (plan) => await GoToDetails(plan));
            ViewWorkoutDetailsCommand = new Command<WorkoutPlan>(async (plan) => await GoToWorkoutDetails(plan));
            ToggleFavoriteCommand = new Command<MealPlan>(async (plan) => await ToggleFavoriteAsync(plan));
            SwitchTabCommand = new Command<string>((tab) => 
            {
                IsMealPlanTab = tab == "MealPlan";
                // Завантажуємо дані для вибраної вкладки, якщо їх ще немає
                _ = LoadHistoryAsync(false);
            });
        }

        private async Task ToggleFavoriteAsync(MealPlan plan)
        {
            if (plan == null) return;
            
            // Оптимістичне оновлення UI
            plan.IsFavorite = !plan.IsFavorite;

            // Якщо ми у режимі 'Тільки вибрані' і забрали лайк, треба видалити зі списку
            if (ShowFavoritesOnly && !plan.IsFavorite)
            {
                FilteredMealPlans.Remove(plan);
            }

            // Відправляємо запит на сервер
            var success = await _apiService.ToggleMealPlanFavoriteAsync(plan.Id);
            if (!success)
            {
                // Відкат, якщо помилка
                plan.IsFavorite = !plan.IsFavorite;
                if (ShowFavoritesOnly && plan.IsFavorite && !FilteredMealPlans.Contains(plan))
                {
                    FilteredMealPlans.Add(plan);
                }
            }
        }

        private void ApplyFilter()
        {
            FilteredMealPlans.Clear();
            var sorted = MealPlans.OrderByDescending(p => p.IsFavorite).ThenByDescending(p => p.Date);
            
            foreach (var plan in sorted)
            {
                if (ShowFavoritesOnly && !plan.IsFavorite) continue;
                FilteredMealPlans.Add(plan);
            }
        }

        public async Task LoadHistoryAsync(bool forceRefresh = false)
        {
            if (IsLoading) return;

            // Перевірка наявності даних для поточної вкладки
            if (!forceRefresh)
            {
                if (IsMealPlanTab && MealPlans.Count > 0) return;
                if (!IsMealPlanTab && Workouts.Count > 0) return;
            }

            try
            {
                IsLoading = true;
                int userId = Preferences.Get("UserId", 0);
                if (userId > 0)
                {
                    if (IsMealPlanTab)
                    {
                         var plans = await _apiService.GetMealPlanHistoryAsync(userId);
                         MealPlans.Clear();
                         foreach (var plan in plans) MealPlans.Add(plan);
                         ApplyFilter();
                    }
                    else
                    {
                         var workouts = await _apiService.GetUserWorkoutsAsync(userId);
                         Workouts.Clear();
                         foreach (var work in workouts) Workouts.Add(work);
                    }
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
            Workouts.Clear();
        }

        async Task GoToWorkoutDetails(WorkoutPlan plan)
        {
             if (plan == null) return;
             
             await Shell.Current.GoToAsync(nameof(WorkoutDetailPage), new Dictionary<string, object>
             {
                 { "WorkoutJson", plan.PlanText ?? "" },
                 { "Goal", plan.Goal ?? "Тренування" }
             });
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