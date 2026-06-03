using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class SavedRecipesViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        public ObservableCollection<ApiService.MealItem> SavedRecipes { get; } = new();

        public ICommand DeleteRecipeCommand { get; }
        public ICommand LogRecipeCommand { get; }

        public SavedRecipesViewModel(ApiService apiService)
        {
            _apiService = apiService;
            DeleteRecipeCommand = new Command<ApiService.MealItem>(DeleteRecipe);
            LogRecipeCommand = new Command<ApiService.MealItem>(async (recipe) => await LogRecipeAsync(recipe));
        }

        public void LoadRecipes()
        {
            try
            {
                var savedJson = Preferences.Get("SavedRecipes", "");
                if (!string.IsNullOrEmpty(savedJson))
                {
                    var savedRecipes = System.Text.Json.JsonSerializer.Deserialize<List<ApiService.MealItem>>(savedJson);
                    SavedRecipes.Clear();
                    if (savedRecipes != null)
                    {
                        foreach (var recipe in savedRecipes)
                        {
                            SavedRecipes.Add(recipe);
                        }
                    }
                }
            }
            catch { }
        }

        private async void DeleteRecipe(ApiService.MealItem recipe)
        {
            if (recipe == null) return;
            
            bool confirm = await Application.Current.MainPage.DisplayAlert("Підтвердження", $"Ви дійсно хочете видалити рецепт '{recipe.Name}'?", "Так", "Ні");
            if (!confirm) return;

            SavedRecipes.Remove(recipe);
            
            try
            {
                var newJson = System.Text.Json.JsonSerializer.Serialize(SavedRecipes.ToList());
                Preferences.Set("SavedRecipes", newJson);
            }
            catch { }
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
