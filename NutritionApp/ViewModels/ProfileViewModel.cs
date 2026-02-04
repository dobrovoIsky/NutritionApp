using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        // Кешування - завантажуємо тільки раз
        private bool _isDataLoaded = false;
        private int _lastUserId = 0;

        // Аватари
        public ObservableCollection<AvatarOption> AvatarOptions { get; } = new()
        {
            new AvatarOption { Id = 1, ImageSource = "avatar1.png" },
            new AvatarOption { Id = 2, ImageSource = "avatar2.png" },
            new AvatarOption { Id = 3, ImageSource = "avatar3.png" },
            new AvatarOption { Id = 4, ImageSource = "avatar4.png" },
            new AvatarOption { Id = 5, ImageSource = "avatar5.png" },
            new AvatarOption { Id = 6, ImageSource = "avatar6.png" },
            new AvatarOption { Id = 7, ImageSource = "avatar7.png" },
            new AvatarOption { Id = 8, ImageSource = "avatar8.png" }
        };

        private AvatarOption _selectedAvatar;
        private bool _isAvatarChanging = false;

        public AvatarOption SelectedAvatar
        {
            get => _selectedAvatar;
            set
            {
                if (_selectedAvatar != value)
                {
                    _selectedAvatar = value;
                    OnPropertyChanged();
                    if (!_isAvatarChanging && value != null)
                    {
                        _ = SaveAvatarAsync();
                    }
                }
            }
        }

        public ICommand LoadUserProfileCommand { get; }

        public ProfileViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadUserProfileCommand = new Command(async () => await LoadUserProfileAsync(forceRefresh: false));
        }

        public async Task LoadUserProfileAsync(bool forceRefresh = false)
        {
            int userId = Preferences.Get("UserId", 0);

            // Якщо той самий користувач і дані вже є - пропускаємо
            if (!forceRefresh && _isDataLoaded && _lastUserId == userId && UserProfile != null)
                return;

            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Debug.WriteLine($"ProfileViewModel: Loading profile for userId: {userId}");

                if (userId > 0)
                {
                    UserProfile = await _apiService.GetUserProfileAsync(userId);
                    _lastUserId = userId;
                    _isDataLoaded = true;
                    Debug.WriteLine($"ProfileViewModel: Loaded - Height={UserProfile?.Height}, Weight={UserProfile?.Weight}");
                }
            }
            finally
            {
                IsLoading = false;
            }

            // Встановлюємо аватар
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

                var payload = new
                {
                    Height = UserProfile.Height,
                    Weight = UserProfile.Weight,
                    Age = UserProfile.Age,
                    Goal = UserProfile.Goal ?? string.Empty,
                    ActivityLevel = UserProfile.ActivityLevel ?? string.Empty,
                    AvatarId = SelectedAvatar.Id,
                    Gender = UserProfile.Gender ?? "male"
                };

                var updatedProfile = await _apiService.UpdateUserProfileAsync(UserProfile.Id, payload);

                if (updatedProfile != null)
                {
                    UserProfile = updatedProfile;
                    Debug.WriteLine($"ProfileViewModel: Avatar saved successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProfileViewModel: SaveAvatarAsync exception: {ex.Message}");
            }
        }

        // Скидання при логауті
        public void Reset()
        {
            _isDataLoaded = false;
            _lastUserId = 0;
            UserProfile = null;
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
        public string ImageSource { get; set; }
    }
}