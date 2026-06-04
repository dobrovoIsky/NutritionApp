using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class MealGroup : ObservableCollection<FoodEntry>
    {
        public string Name { get; set; }
        
        public double TotalCalories => this.Sum(x => x.Calories);

        public MealGroup(string name, IEnumerable<FoodEntry> items) : base(items)
        {
            Name = name;
        }
        
        public void UpdateCalories()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(TotalCalories)));
        }
    }

    [QueryProperty(nameof(RefreshTracker), "refreshTracker")]
    public class MyRationViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        
        public ObservableCollection<MealGroup> MealGroups { get; } = new ObservableCollection<MealGroup>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _refreshTracker;
        public string RefreshTracker
        {
            get => _refreshTracker;
            set
            {
                _refreshTracker = value;
                if (value == "true")
                {
                    _ = LoadDataAsync();
                }
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddFoodCommand { get; }
        public ICommand EditFoodCommand { get; }
        public ICommand DeleteFoodCommand { get; }
        public ICommand GoBackCommand { get; }

        public MyRationViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadDataCommand = new Command(async () => await LoadDataAsync());
            AddFoodCommand = new Command<string>(async (mealType) => await GoToAddFood(mealType));
            EditFoodCommand = new Command<FoodEntry>(async (entry) => await EditFoodAsync(entry));
            DeleteFoodCommand = new Command<FoodEntry>(async (entry) => await DeleteFoodAsync(entry));
            GoBackCommand = new Command(async () => await Shell.Current.Navigation.PopAsync());
            
            InitializeGroups();
        }

        private void InitializeGroups()
        {
            MealGroups.Clear();
            MealGroups.Add(new MealGroup("Сніданок", new List<FoodEntry>()));
            MealGroups.Add(new MealGroup("Другий сніданок", new List<FoodEntry>()));
            MealGroups.Add(new MealGroup("Обід", new List<FoodEntry>()));
            MealGroups.Add(new MealGroup("Перекус", new List<FoodEntry>()));
            MealGroups.Add(new MealGroup("Вечеря", new List<FoodEntry>()));
        }

        public async Task LoadDataAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                int userId = Microsoft.Maui.Storage.Preferences.Get("UserId", 0);
                if (userId == 0) return;

                var entries = await _apiService.GetDailyFoodEntriesAsync(userId);
                
                InitializeGroups(); // Reset

                foreach (var entry in entries)
                {
                    var mealType = string.IsNullOrWhiteSpace(entry.MealType) ? "Перекус" : entry.MealType;
                    var group = MealGroups.FirstOrDefault(g => g.Name == mealType);
                    if (group != null)
                    {
                        group.Add(entry);
                        group.UpdateCalories();
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool _isNavigating;
        private async Task GoToAddFood(string mealType)
        {
            if (_isNavigating) return;
            _isNavigating = true;
            try 
            { 
                var encodedMealType = Uri.EscapeDataString(mealType ?? "Перекус");
                await Shell.Current.GoToAsync($"{nameof(Views.AddFoodPage)}?mealType={encodedMealType}");
            }
            finally { await Task.Delay(500); _isNavigating = false; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async Task EditFoodAsync(FoodEntry entry)
        {
            if (entry == null) return;
            
            var navigationParameter = new Dictionary<string, object>
            {
                { "foodEntry", entry }
            };
            await Shell.Current.GoToAsync($"{nameof(Views.AddFoodPage)}", navigationParameter);
        }

        private async Task DeleteFoodAsync(FoodEntry entry)
        {
            if (entry == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Видалення", $"Видалити {entry.Name}?", "Так", "Ні");
            if (!confirm) return;

            var success = await _apiService.DeleteFoodEntryAsync(entry.Id, entry.UserId);
            if (success)
            {
                // Remove from local collection
                var group = MealGroups.FirstOrDefault(g => g.Contains(entry));
                if (group != null)
                {
                    group.Remove(entry);
                    group.UpdateCalories();
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося видалити запис", "ОК");
            }
        }
    }
}
