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
                OnPropertyChanged(nameof(IsNotLoading));
                OnPropertyChanged(nameof(ShowInitialMessage));
            }
        }

        public bool IsNotLoading => !IsLoading;

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

        // Побажання користувача
        private string _preferencesText;
        public string PreferencesText
        {
            get => _preferencesText;
            set
            {
                _preferencesText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ApiService.MealItem> Recipes { get; } = new();

        public ICommand GeneratePlanCommand { get; }
        public ICommand OpenProductSelectionCommand { get; }
        public ICommand RemoveFoodCommand { get; }
        public ICommand LogRecipeCommand { get; }
        public ICommand SaveRecipeCommand { get; }

        public MealPlanViewModel(ApiService apiService)
        {
            _apiService = apiService;
            GeneratePlanCommand = new Command(async () => await GeneratePlanAsync());
            OpenProductSelectionCommand = new Command(async () => await OpenProductSelectionAsync());
            RemoveFoodCommand = new Command<ApiService.FoodItem>(RemoveFood);
            LogRecipeCommand = new Command<ApiService.MealItem>(async (recipe) => await LogRecipeAsync(recipe));
            SaveRecipeCommand = new Command<ApiService.MealItem>(async (recipe) => await SaveRecipeAsync(recipe));
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
                Recipes.Clear();

                int userId = Preferences.Get("UserId", 0);
                if (userId > 0)
                {
                    // Check balance first
                    var profile = await _apiService.GetUserProfileAsync(userId, forceRefresh: true);
                    if (profile == null || profile.Balance < 10)
                    {
                        ErrorMessage = "💎 Недостатньо балів для генерації рецепту. (Потрібно 10)";
                        HasError = true;
                        IsLoading = false;
                        return;
                    }

                    // Передаємо вибрані продукти та побажання в API
                    var result = await _apiService.GenerateMealPlanAsync(userId, _selectedProducts, PreferencesText);

                    if (result != null && result.Meals != null && result.Meals.Count > 0)
                    {
                        Summary = result.Summary ?? "Ваші персональні рецепти готові!";
                        foreach (var recipe in result.Meals)
                        {
                            Recipes.Add(recipe);
                        }
                        HasData = true;
                    }
                    else
                    {
                        ErrorMessage = "Не вдалося отримати рецепти.";
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

        private void RemoveFood(ApiService.FoodItem food)
        {
            if (food == null || Recipes == null) return;
            
            foreach (var recipe in Recipes)
            {
                if (recipe.Foods != null && recipe.Foods.Contains(food))
                {
                    recipe.Foods.Remove(food);
                    // Перераховуємо калорії для рецепту
                    recipe.TotalCalories = recipe.Foods.Sum(f => f.Calories);
                    break;
                }
            }
        }

        private async Task LogRecipeAsync(ApiService.MealItem recipe)
        {
            if (recipe == null) return;
            try
            {
                int userId = Preferences.Get("UserId", 0);
                if (userId == 0) return;

                // Запитуємо у користувача куди додати рецепт
                string action = await Application.Current.MainPage.DisplayActionSheet("Куди додати цю страву?", "Скасувати", null, "Сніданок", "Обід", "Вечеря", "Перекус");
                
                if (action == "Скасувати" || string.IsNullOrEmpty(action))
                {
                    return; // Користувач скасував
                }

                // Логуємо як ОДНУ страву (Dish) з сумарними показниками
                var totalCalories = recipe.Foods?.Sum(f => f.Calories) ?? recipe.TotalCalories;
                var totalProtein = recipe.Foods?.Sum(f => f.Protein) ?? 0;
                var totalFat = recipe.Foods?.Sum(f => f.Fat) ?? 0;
                var totalCarbs = recipe.Foods?.Sum(f => f.Carbs) ?? 0;
                var totalWeight = recipe.Foods?.Sum(f => 
                {
                    var weightStr = new string(f.Weight?.Where(char.IsDigit).ToArray());
                    double.TryParse(weightStr, out double w);
                    return w;
                }) ?? 100;

                var entry = new Models.FoodEntry
                {
                    UserId = userId,
                    Name = recipe.Name,
                    Calories = totalCalories,
                    Protein = totalProtein,
                    Fat = totalFat,
                    Carbs = totalCarbs,
                    Weight = totalWeight > 0 ? totalWeight : 100, // Default 100
                    MealType = action, 
                    LoggedAt = DateTime.UtcNow
                };

                var saved = await _apiService.LogFoodAsync(entry);
                if (saved != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Успіх", $"Страва '{recipe.Name}' додана до щоденника!", "OK");
                    MessagingCenter.Send(this, "FoodLogged");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося зберегти страву.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "ОК");
            }
        }

        private async Task SaveRecipeAsync(ApiService.MealItem recipe)
        {
            if (recipe == null) return;
            try
            {
                var savedJson = Preferences.Get("SavedRecipes", "");
                var savedRecipes = string.IsNullOrEmpty(savedJson) 
                    ? new List<ApiService.MealItem>() 
                    : System.Text.Json.JsonSerializer.Deserialize<List<ApiService.MealItem>>(savedJson);
                
                if (savedRecipes == null) savedRecipes = new List<ApiService.MealItem>();
                
                if (savedRecipes.Any(r => r.Name == recipe.Name))
                {
                    await Application.Current.MainPage.DisplayAlert("Увага", "Цей рецепт вже збережено!", "OK");
                    return;
                }
                
                savedRecipes.Add(recipe);
                var newJson = System.Text.Json.JsonSerializer.Serialize(savedRecipes);
                Preferences.Set("SavedRecipes", newJson);
                
                await Application.Current.MainPage.DisplayAlert("Успіх", "Рецепт збережено у вкладку 'Рецепти'!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Не вдалося зберегти рецепт: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}