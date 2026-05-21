using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class AddFoodViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private string _mealType;
        public string MealType
        {
            get => _mealType;
            set { _mealType = value; OnPropertyChanged(); }
        }
        private readonly ApiService _apiService;
        private List<FoodDatabaseItem> _allProducts = new();
        
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }
        
        private string _calories;
        public string Calories { get => _calories; set { _calories = value; OnPropertyChanged(); } }
        
        private string _protein;
        public string Protein { get => _protein; set { _protein = value; OnPropertyChanged(); } }
        
        private string _fat;
        public string Fat { get => _fat; set { _fat = value; OnPropertyChanged(); } }
        
        private string _carbs;
        public string Carbs { get => _carbs; set { _carbs = value; OnPropertyChanged(); } }
        
        private string _weight;
        public string Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                OnPropertyChanged();
                RecalculateNutrients();
            }
        }

        private FoodDatabaseItem _selectedProduct;
        public FoodDatabaseItem SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                if (_selectedProduct != null)
                {
                    Name = _selectedProduct.Name;
                    ShowDropdown = false;
                    RecalculateNutrients();
                }
            }
        }

        private bool _showDropdown;
        public bool ShowDropdown
        {
            get => _showDropdown;
            set { _showDropdown = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FoodDatabaseItem> FilteredProducts { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public AddFoodViewModel(ApiService apiService)
        {
            _apiService = apiService;
            SaveCommand = new Command(async () => await SaveFoodAsync());
            
            // Load products in background
            _ = LoadProductsAsync();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("mealType"))
            {
                MealType = query["mealType"]?.ToString();
            }
        }

        private async Task LoadProductsAsync()
        {
            var products = await _apiService.GetFoodDatabaseItemsAsync();
            if (products != null && products.Any())
            {
                _allProducts = products;
            }
        }

        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(Name) || SelectedProduct?.Name == Name)
            {
                ShowDropdown = false;
                return;
            }

            var lowerQuery = Name.ToLowerInvariant();
            var matches = _allProducts.Where(p => p.Name.ToLowerInvariant().Contains(lowerQuery)).Take(10).ToList();
            
            FilteredProducts.Clear();
            foreach (var match in matches)
            {
                FilteredProducts.Add(match);
            }
            
            ShowDropdown = FilteredProducts.Any();
        }

        private void RecalculateNutrients()
        {
            if (SelectedProduct == null) return;

            if (double.TryParse(Weight, out double weight) && weight > 0)
            {
                double multiplier = weight / 100.0;
                Calories = Math.Round(SelectedProduct.CaloriesPer100g * multiplier, 1).ToString();
                Protein = Math.Round(SelectedProduct.ProteinPer100g * multiplier, 1).ToString();
                Fat = Math.Round(SelectedProduct.FatPer100g * multiplier, 1).ToString();
                Carbs = Math.Round(SelectedProduct.CarbsPer100g * multiplier, 1).ToString();
            }
            else
            {
                // Default to 100g values if weight is empty/invalid
                Calories = SelectedProduct.CaloriesPer100g.ToString();
                Protein = SelectedProduct.ProteinPer100g.ToString();
                Fat = SelectedProduct.FatPer100g.ToString();
                Carbs = SelectedProduct.CarbsPer100g.ToString();
            }
        }

        private async Task SaveFoodAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Calories))
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Введіть назву та калорії", "ОК");
                return;
            }

            if (IsLoading) return;
            IsLoading = true;

            try
            {
                int userId = Preferences.Get("UserId", 0);
                if (userId == 0) return;

                var entry = new FoodEntry
                {
                    UserId = userId,
                    Name = Name,
                    Calories = double.TryParse(Calories, out var cal) ? cal : 0,
                    Protein = double.TryParse(Protein, out var pro) ? pro : 0,
                    Fat = double.TryParse(Fat, out var f) ? f : 0,
                    Carbs = double.TryParse(Carbs, out var c) ? c : 0,
                    Weight = double.TryParse(Weight, out var w) ? w : 0,
                    MealType = this.MealType ?? string.Empty,
                    LoggedAt = DateTime.UtcNow
                };

                var saved = await _apiService.LogFoodAsync(entry);
                if (saved != null)
                {
                    await Shell.Current.Navigation.PopAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося зберегти дані", "ОК");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
