using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class WorkoutViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public ObservableCollection<WorkoutPlan> WorkoutPlans { get; } = new();

        // Поля вводу
        private string _goal = "схуднення";
        public string Goal
        {
            get => _goal;
            set { _goal = value; OnPropertyChanged(); }
        }

        private string _intensity = "medium";
        public string Intensity
        {
            get => _intensity;
            set { _intensity = value; OnPropertyChanged(); }
        }

        private int _durationMinutes = 45;
        public int DurationMinutes
        {
            get => _durationMinutes;
            set { _durationMinutes = value; OnPropertyChanged(); }
        }

        private string _generatedPlanText;
        public string GeneratedPlanText
        {
            get => _generatedPlanText;
            set { _generatedPlanText = value; OnPropertyChanged(); }
        }

        public bool IsBusy { get; set; }

        public ICommand GenerateCommand { get; }
        public ICommand LoadHistoryCommand { get; }

        public WorkoutViewModel()
        {
            _apiService = new ApiService();
            GenerateCommand = new Command(async () => await GenerateAsync());
            LoadHistoryCommand = new Command(async () => await LoadHistoryAsync());
        }

        private async Task GenerateAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var userIdStr = Preferences.Get("UserId", "0");
                if (!int.TryParse(userIdStr, out var userId) || userId == 0)
                {
                    GeneratedPlanText = "Помилка: не знайдено UserId.";
                    return;
                }

                // Pass the required arguments directly to the method
                var plan = await _apiService.GenerateWorkoutAsync(userId, Goal, Intensity, DurationMinutes);

                GeneratedPlanText = plan.PlanText;
                WorkoutPlans.Insert(0, plan);
            }
            catch (Exception ex)
            {
                GeneratedPlanText = $"Помилка: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadHistoryAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var userIdStr = Preferences.Get("UserId", "0");
                if (!int.TryParse(userIdStr, out var userId) || userId == 0)
                    return;

                WorkoutPlans.Clear();
                var list = await _apiService.GetUserWorkoutsAsync(userId);

                foreach (var workout in list)
                    WorkoutPlans.Add(workout);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
