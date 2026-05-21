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
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private const double CaloriesPerStep = 0.04;
        private const int DailyStepsGoal = 10_000;

        private readonly ApiService _apiService;
        private readonly IPedometerService _pedometerService;

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

            OnPropertyChanged(nameof(NetCalories));
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
