using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public AvatarOption SelectedAvatar
        {
            get => _selectedAvatar;
            set { _selectedAvatar = value; OnPropertyChanged(); SaveAvatarAsync(); }
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

            // Встановлюємо вибраний аватар після завантаження
            if (UserProfile != null)
            {
                SelectedAvatar = AvatarOptions.FirstOrDefault(a => a.Id == UserProfile.AvatarId);
            }
        }

        private async Task SaveAvatarAsync()
        {
            if (SelectedAvatar == null || UserProfile == null) return;
            UserProfile.AvatarId = SelectedAvatar.Id;
            await _apiService.UpdateUserProfileAsync(UserProfile.Id, new { avatarId = SelectedAvatar.Id });
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