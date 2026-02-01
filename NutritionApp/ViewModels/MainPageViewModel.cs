using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;
using NutritionApp.Views;
using Microsoft.Maui.Storage;

namespace NutritionApp.ViewModels
{
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
                bool hasKey = Preferences.ContainsKey("UserId");
                int userId = Preferences.Get("UserId", 0);
                Debug.WriteLine($"Preferences contains UserId key: {hasKey}; value read: {userId}");
                if (userId > 0)
                {
                    UserProfile = await _apiService.GetUserProfileAsync(userId);
                    Debug.WriteLine($"Profile loaded: Bju.Calories = {UserProfile?.Bju?.Calories}");
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