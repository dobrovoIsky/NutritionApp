using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Services;
using NutritionApp.Views;

namespace NutritionApp.ViewModels
{
    public class MealPlanViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private List<string> _selectedProducts = new();

        private string _summary;
        public string Summary
        {
            get => _summary;
            set { _summary = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowInitialMessage));
            }
        }

        private bool _hasData;
        public bool HasData
        {
            get => _hasData;
            set
            {
                _hasData = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowInitialMessage));
            }
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowInitialMessage));
            }
        }

        // Властивість для показу початкового повідомлення
        public bool ShowInitialMessage => !IsLoading && !HasData && !HasError;

        // Властивості для вибору продуктів
        public bool HasSelectedProducts => _selectedProducts.Count > 0;
        public int SelectedProductsCount => _selectedProducts.Count;
        public string SelectedProductsText => _selectedProducts.Count > 0 
            ? $"Обрано {_selectedProducts.Count} продуктів" 
            : "Натисніть, щоб обрати";

        public ObservableCollection<ApiService.MealItem> Meals { get; } = new();

        public ICommand GeneratePlanCommand { get; }
        public ICommand OpenProductSelectionCommand { get; }
        public ICommand LogFoodFromPlanCommand { get; }

        public MealPlanViewModel(ApiService apiService)
        {
            _apiService = apiService;
            GeneratePlanCommand = new Command(async () => await GeneratePlanAsync());
            OpenProductSelectionCommand = new Command(async () => await OpenProductSelectionAsync());
            LogFoodFromPlanCommand = new Command<ApiService.FoodItem>(async (food) => await LogFoodFromPlanAsync(food));
            HasData = false;
            HasError = false;
            
            // Завантажуємо збережені продукти
            LoadSelectedProducts();
        }

        private void LoadSelectedProducts()
        {
            try
            {
                var favoritesJson = Preferences.Get("FavoriteProducts", "");
                if (!string.IsNullOrEmpty(favoritesJson))
                {
                    var favorites = System.Text.Json.JsonSerializer.Deserialize<List<string>>(favoritesJson);
                    if (favorites != null)
                    {
                        _selectedProducts = favorites;
                        OnPropertyChanged(nameof(HasSelectedProducts));
                        OnPropertyChanged(nameof(SelectedProductsCount));
                        OnPropertyChanged(nameof(SelectedProductsText));
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        private async Task OpenProductSelectionAsync()
        {
            var productSelectionPage = new ProductSelectionPage(_selectedProducts, OnProductsSelected);
            await Shell.Current.Navigation.PushAsync(productSelectionPage);
        }

        private void OnProductsSelected(List<string> selectedProducts)
        {
            _selectedProducts = selectedProducts ?? new List<string>();
            OnPropertyChanged(nameof(HasSelectedProducts));
            OnPropertyChanged(nameof(SelectedProductsCount));
            OnPropertyChanged(nameof(SelectedProductsText));
        }

        private async Task GeneratePlanAsync()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                HasError = false;
                HasData = false;
                ErrorMessage = "";
                Summary = "";
                Meals.Clear();

                int userId = Preferences.Get("UserId", 0);
                if (userId > 0)
                {
                    // Передаємо вибрані продукти в API
                    var result = await _apiService.GenerateMealPlanAsync(userId, _selectedProducts);

                    if (result != null && result.Meals != null && result.Meals.Count > 0)
                    {
                        Summary = result.Summary ?? "Ваш персональний план харчування готовий!";
                        foreach (var meal in result.Meals)
                        {
                            Meals.Add(meal);
                        }
                        HasData = true;
                    }
                    else
                    {
                        ErrorMessage = "Не вдалося отримати план харчування.";
                        HasError = true;
                    }
                }
                else
                {
                    ErrorMessage = "Помилка: не вдалося ідентифікувати користувача.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Сталася помилка: {ex.Message}";
                HasError = true;
                HasData = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LogFoodFromPlanAsync(ApiService.FoodItem food)
        {
            if (food == null) return;
            try
            {
                int userId = Preferences.Get("UserId", 0);
                if (userId == 0) return;

                // Extract numeric weight if possible
                double weight = 0;
                var weightStr = new string(food.Weight?.Where(char.IsDigit).ToArray());
                double.TryParse(weightStr, out weight);

                var entry = new Models.FoodEntry
                {
                    UserId = userId,
                    Name = food.Name,
                    Calories = food.Calories,
                    Protein = food.Protein,
                    Fat = food.Fat,
                    Carbs = food.Carbs,
                    Weight = weight > 0 ? weight : 100, // Default 100 if couldn't parse
                    LoggedAt = DateTime.UtcNow
                };

                var saved = await _apiService.LogFoodAsync(entry);
                if (saved != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Успіх", $"'{food.Name}' додано до трекера!", "ОК");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося зберегти дані", "ОК");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", ex.Message, "ОК");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}