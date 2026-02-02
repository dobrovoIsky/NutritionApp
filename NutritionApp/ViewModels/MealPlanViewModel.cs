using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class MealPlanViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

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

        public ObservableCollection<ApiService.MealItem> Meals { get; } = new();

        public ICommand GeneratePlanCommand { get; }

        public MealPlanViewModel(ApiService apiService)
        {
            _apiService = apiService;
            GeneratePlanCommand = new Command(async () => await GeneratePlanAsync());
            HasData = false;
            HasError = false;
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
                    var result = await _apiService.GenerateMealPlanAsync(userId);

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}