using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    [QueryProperty(nameof(UserProfile), "UserProfile")]
    public class EditProfileViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        private UserProfile _userProfile;
        public UserProfile UserProfile
        {
            get => _userProfile;
            set { _userProfile = value; OnPropertyChanged(); }
        }

        public ICommand SaveProfileCommand { get; }

        public EditProfileViewModel(ApiService apiService)
        {
            _apiService = apiService;
            SaveProfileCommand = new Command(async () => await SaveProfileAsync());
        }

        private async Task SaveProfileAsync()
        {
            if (UserProfile == null) return;
            var updateDto = new
            {
                height = UserProfile.Height,
                weight = UserProfile.Weight,
                age = UserProfile.Age,
                goal = UserProfile.Goal,
                activityLevel = UserProfile.ActivityLevel
            };
            var updatedProfile = await _apiService.UpdateUserProfileAsync(UserProfile.Id, updateDto);
            if (updatedProfile != null)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося оновити профіль", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}