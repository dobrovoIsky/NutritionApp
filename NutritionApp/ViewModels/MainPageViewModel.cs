using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;
using NutritionApp.Views;

namespace NutritionApp.ViewModels
{
    // Повертаємо реалізацію INotifyPropertyChanged
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

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

        public ICommand LoadDataCommand { get; }
        public ICommand GoToEditProfileCommand { get; }
        public ICommand GoToMealPlanCommand { get; }

        public MainPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadDataCommand = new Command(async () => await LoadUserProfileAsync());
            GoToEditProfileCommand = new Command(async () => await GoToEditProfile());
            GoToMealPlanCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(MealPlanPage)));
        }

        public async Task LoadUserProfileAsync()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                int userId = Preferences.Get("userId", 0);
                if (userId > 0)
                {
                    UserProfile = await _apiService.GetUserProfileAsync(userId);
                }
            }
            finally
            {
                IsLoading = false;
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

        // Повертаємо реалізацію INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}