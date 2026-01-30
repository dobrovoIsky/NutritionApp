using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class MealPlanViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        private string _mealPlanText;
        public string MealPlanText
        {
            get => _mealPlanText;
            set { _mealPlanText = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand GeneratePlanCommand { get; }

        public MealPlanViewModel(ApiService apiService)
        {
            _apiService = apiService;
            GeneratePlanCommand = new Command(async () => await GeneratePlanAsync());
            MealPlanText = "Натисніть кнопку, щоб згенерувати план харчування на основі ваших даних.";
        }

        private async Task GeneratePlanAsync()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                MealPlanText = "";
                int userId = Preferences.Get("UserId", 0);
                if (userId > 0)
                {
                    var result = await _apiService.GenerateMealPlanAsync(userId);
                    // Виправлено: якщо повертається MealPlan з властивістю Plan
                    MealPlanText = result?.Plan ?? "Не вдалося отримати план харчування.";
                }
                else
                {
                    MealPlanText = "Помилка: не вдалося ідентифікувати користувача.";
                }
            }
            catch (Exception ex)
            {
                MealPlanText = $"Сталася помилка: {ex.Message}";
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