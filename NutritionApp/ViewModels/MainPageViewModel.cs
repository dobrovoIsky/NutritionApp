using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;
using NutritionApp.Views;
using Microsoft.Maui.Storage;

namespace NutritionApp.ViewModels
{
    public class DailyStatItem
    {
        public string Day { get; set; }
        public double Value { get; set; }
        public bool IsToday { get; set; }
        public double BarHeight { get; set; }
    }

    public class MainPageViewModel : INotifyPropertyChanged
    {
        private const double CaloriesPerStep = 0.04;
        private const int DailyStepsGoal = 10_000;

        private readonly ApiService _apiService;
        private readonly IPedometerService _pedometerService;

        public System.Collections.ObjectModel.ObservableCollection<DailyStatItem> WeeklyStats { get; } = new();

        private UserProfile _userProfile;
        public UserProfile UserProfile
        {
            get => _userProfile;
            set { _userProfile = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private DailySummary _dailySummary;
        public DailySummary DailySummary
        {
            get => _dailySummary;
            set { _dailySummary = value; OnPropertyChanged(); }
        }

        private System.Collections.ObjectModel.ObservableCollection<FoodEntry> _foodEntries;
        public System.Collections.ObjectModel.ObservableCollection<FoodEntry> FoodEntries
        {
            get => _foodEntries;
            set { _foodEntries = value; OnPropertyChanged(); }
        }

        private double _caloriesProgress;
        public double CaloriesProgress { get => _caloriesProgress; set { _caloriesProgress = value; OnPropertyChanged(); } }

        private double _proteinProgress;
        public double ProteinProgress { get => _proteinProgress; set { _proteinProgress = value; OnPropertyChanged(); } }

        private double _fatProgress;
        public double FatProgress { get => _fatProgress; set { _fatProgress = value; OnPropertyChanged(); } }

        private double _carbsProgress;
        public double CarbsProgress { get => _carbsProgress; set { _carbsProgress = value; OnPropertyChanged(); } }

        private int _todaySteps;
        public int TodaySteps
        {
            get => _todaySteps;
            set { _todaySteps = value; OnPropertyChanged(); OnPropertyChanged(nameof(StepsProgress)); }
        }

        private double _stepCaloriesBurned;
        public double StepCaloriesBurned
        {
            get => _stepCaloriesBurned;
            set { _stepCaloriesBurned = value; OnPropertyChanged(); }
        }

        public double NetCalories =>
            DailySummary == null ? 0 : Math.Max(0, DailySummary.TotalCalories - StepCaloriesBurned);

        public double StepsProgress =>
            DailyStepsGoal > 0 ? Math.Min(1, (double)TodaySteps / DailyStepsGoal) : 0;

        public bool ShowStepsTile =>
            DeviceInfo.Platform == DevicePlatform.Android && _pedometerService.IsSupported;

        public int CarbsColumnSpan => ShowStepsTile ? 1 : 2;

        public ICommand LoadDataCommand { get; }
        public ICommand GoToEditProfileCommand { get; }
        public ICommand GoToMealPlanCommand { get; }
        public ICommand GoToMyRationCommand { get; }

        public MainPageViewModel(ApiService apiService, IPedometerService pedometerService)
        {
            _apiService = apiService;
            _pedometerService = pedometerService;
            _pedometerService.StepsChanged += OnStepsChanged;

            LoadDataCommand = new Command(async () => await LoadUserProfileAsync());
            GoToEditProfileCommand = new Command(async () => await GoToEditProfile());
            GoToMealPlanCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(MealPlanPage)));
            GoToMyRationCommand = new Command(async () => await GoToMyRationAsync());
        }

        public async Task StartPedometerAsync()
        {
            if (DeviceInfo.Platform != DevicePlatform.Android)
                return;

            await _pedometerService.StartAsync();
            TodaySteps = _pedometerService.TodaySteps;
            OnPropertyChanged(nameof(ShowStepsTile));
            OnPropertyChanged(nameof(CarbsColumnSpan));
            UpdateProgress();
        }

        public void StopPedometer() => _pedometerService.Stop();

        private void OnStepsChanged(object? sender, int steps)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TodaySteps = steps;
                UpdateProgress();
            });
        }

        private bool _isNavigating;
        private async Task GoToMyRationAsync()
        {
            if (_isNavigating) return;
            _isNavigating = true;
            try { await Shell.Current.GoToAsync(nameof(MyRationPage)); }
            finally { await Task.Delay(500); _isNavigating = false; }
        }

        public async Task LoadUserProfileAsync()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                bool hasKey = Preferences.ContainsKey("UserId");
                int userId = Preferences.Get("UserId", 0);
                Debug.WriteLine($"Preferences contains UserId key: {hasKey}; value read: {userId}");
                if (userId > 0)
                {
                    UserProfile = await _apiService.GetUserProfileAsync(userId);
                    Debug.WriteLine($"Profile loaded: Bju.Calories = {UserProfile?.Bju?.Calories}");

                    await LoadTrackerDataAsync(userId);
                }
                else
                {
                    Debug.WriteLine("UserId is 0 — profile will not be loaded.");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadTrackerDataAsync(int userId)
        {
            DailySummary = await _apiService.GetDailySummaryAsync(userId);
            FoodEntries = new System.Collections.ObjectModel.ObservableCollection<FoodEntry>(
                await _apiService.GetDailyFoodEntriesAsync(userId));
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (UserProfile?.Bju == null) return;

            StepCaloriesBurned = TodaySteps * CaloriesPerStep;

            CaloriesProgress = UserProfile.Bju.Calories > 0
                ? NetCalories / UserProfile.Bju.Calories
                : 0;

            ProteinProgress = UserProfile.Bju.Proteins > 0 ? DailySummary.TotalProtein / UserProfile.Bju.Proteins : 0;
            FatProgress = UserProfile.Bju.Fats > 0 ? DailySummary.TotalFat / UserProfile.Bju.Fats : 0;
            CarbsProgress = UserProfile.Bju.Carbs > 0 ? DailySummary.TotalCarbs / UserProfile.Bju.Carbs : 0;

            PopulateWeeklyStats();

            OnPropertyChanged(nameof(NetCalories));
        }

        private void PopulateWeeklyStats()
        {
            WeeklyStats.Clear();
            var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            var todayIndex = (int)DateTime.Today.DayOfWeek - 1;
            if (todayIndex < 0) todayIndex = 6; // Sunday

            var random = new Random();
            double goal = UserProfile.Bju.Calories > 0 ? UserProfile.Bju.Calories : 2000;
            
            // Build the last 7 days ending today
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                var dayIndex = (int)date.DayOfWeek - 1;
                if (dayIndex < 0) dayIndex = 6;

                bool isToday = i == 0;
                double value = isToday ? NetCalories : random.Next((int)(goal * 0.6), (int)(goal * 1.2));
                
                // Calculate BarHeight (max 70px, min 18px to avoid rendering errors with CornerRadius=9)
                double maxVal = goal * 1.2;
                double barHeight = Math.Max(18, Math.Min(1.0, value / maxVal) * 70);

                WeeklyStats.Add(new DailyStatItem
                {
                    Day = days[dayIndex],
                    Value = value,
                    IsToday = isToday,
                    BarHeight = barHeight
                });
            }
        }

        private async Task GoToEditProfile()
        {
            if (UserProfile == null) return;
            await Shell.Current.GoToAsync(nameof(EditProfilePage), new Dictionary<string, object>
            {
                { "UserProfile", UserProfile }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
