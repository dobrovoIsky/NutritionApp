using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
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

        public ObservableCollection<AvatarOption> AvatarOptions { get; } = new()
        {
            new AvatarOption { Id = 1, Name = "Аватар 1", ImageSource = "avatar1.png" },
            new AvatarOption { Id = 2, Name = "Аватар 2", ImageSource = "avatar2.png" },
            new AvatarOption { Id = 3, Name = "Аватар 3", ImageSource = "avatar3.png" },
            new AvatarOption { Id = 4, Name = "Аватар 4", ImageSource = "avatar4.png" }
        };

        private AvatarOption _selectedAvatar;
        private bool _isAvatarChanging = false; // Флаг для уникнення зациклення

        public AvatarOption SelectedAvatar
        {
            get => _selectedAvatar;
            set
            {
                if (_selectedAvatar != value)
                {
                    _selectedAvatar = value;
                    OnPropertyChanged();

                    // Зберігаємо тільки якщо профіль завантажено і це не початкове встановлення
                    if (!_isAvatarChanging && UserProfile != null && UserProfile.Id > 0)
                    {
                        _ = SaveAvatarAsync();
                    }
                }
            }
        }

        public ProfileViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadUserProfileCommand = new Command(async () => await LoadUserProfileAsync());
        }

        public ICommand LoadUserProfileCommand { get; }

        public async Task LoadUserProfileAsync()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                int userId = Preferences.Get("UserId", 0);
                Debug.WriteLine($"ProfileViewModel: Loading profile for userId: {userId}");
                if (userId > 0)
                {
                    UserProfile = await _apiService.GetUserProfileAsync(userId);
                    Debug.WriteLine($"ProfileViewModel: Loaded - Height={UserProfile?.Height}, Weight={UserProfile?.Weight}");
                }
            }
            finally
            {
                IsLoading = false;
            }

            // Встановлюємо вибраний аватар після завантаження (без збереження)
            if (UserProfile != null)
            {
                _isAvatarChanging = true;
                SelectedAvatar = AvatarOptions.FirstOrDefault(a => a.Id == UserProfile.AvatarId);
                _isAvatarChanging = false;
            }
        }

        private async Task SaveAvatarAsync()
        {
            if (SelectedAvatar == null || UserProfile == null || UserProfile.Id <= 0) return;

            try
            {
                Debug.WriteLine($"ProfileViewModel: Saving avatar {SelectedAvatar.Id} for user {UserProfile.Id}");

                // ВАЖЛИВО: Відправляємо ВСІ поля профілю, не тільки avatarId!
                var payload = new
                {
                    Height = UserProfile.Height,
                    Weight = UserProfile.Weight,
                    Age = UserProfile.Age,
                    Goal = UserProfile.Goal ?? string.Empty,
                    ActivityLevel = UserProfile.ActivityLevel ?? string.Empty,
                    AvatarId = SelectedAvatar.Id,  // Новий аватар
                    Gender = UserProfile.Gender ?? "male"
                };

                var updatedProfile = await _apiService.UpdateUserProfileAsync(UserProfile.Id, payload);

                if (updatedProfile != null)
                {
                    UserProfile = updatedProfile;
                    Debug.WriteLine($"ProfileViewModel: Avatar saved successfully");
                }
                else
                {
                    Debug.WriteLine($"ProfileViewModel: Failed to save avatar");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProfileViewModel: SaveAvatarAsync exception: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AvatarOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageSource { get; set; }
    }
}