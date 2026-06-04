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
        private readonly WaterReminderService _waterService;

        private UserProfile _userProfile;
        public UserProfile UserProfile
        {
            get => _userProfile;
            set 
            { 
                _userProfile = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(GoalUa));
                OnPropertyChanged(nameof(ActivityLevelUa));
            }
        }

        public string GoalUa => _userProfile?.Goal switch
        {
            "lose weight" => "Схуднення",
            "gain muscle" => "Набір маси",
            "maintain weight" => "Підтримка ваги",
            _ => _userProfile?.Goal ?? "—"
        };

        public string ActivityLevelUa => _userProfile?.ActivityLevel switch
        {
            "sedentary" => "Сидячий спосіб життя",
            "lightly active" => "Легка активність",
            "moderately active" => "Помірна активність",
            "very active" => "Висока активність",
            "extra active" => "Дуже висока активність",
            _ => _userProfile?.ActivityLevel ?? "—"
        };

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Кешування - завантажуємо тільки раз
        private bool _isDataLoaded = false;
        private int _lastUserId = 0;

        public bool HasAvatar => !string.IsNullOrEmpty(UserProfile?.AvatarBase64);
        public string AvatarInitial => !string.IsNullOrEmpty(UserProfile?.Username) ? UserProfile.Username.Substring(0, 1).ToUpper() : "?";

        public ICommand PickAvatarCommand { get; }

        public ProfileViewModel(ApiService apiService, WaterReminderService waterService)
        {
            _apiService = apiService;
            _waterService = waterService;
            LoadUserProfileCommand = new Command(async () => await LoadUserProfileAsync(forceRefresh: true));
            PickAvatarCommand = new Command(async () => await PickAndUploadAvatarAsync());
        }

        private async Task PickAndUploadAvatarAsync()
        {
            try
            {
                var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Оберіть аватарку"
                });

                if (result != null)
                {
                    IsLoading = true;
                    // Отримуємо потік
                    using var stream = await result.OpenReadAsync();
                    
#if ANDROID || IOS || MACCATALYST || WINDOWS
                    // Зменшуємо зображення, щоб уникнути вильоту через брак пам'яті (Out of Memory)
                    using var image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream);
                    using var resizedImage = image.Downsize(256, 256, true);
                    using var memoryStream = new MemoryStream();
                    resizedImage.Save(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
#else
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
#endif
                    
                    // Конвертуємо в Base64
                    string base64String = System.Convert.ToBase64String(imageBytes);

                    // Відправляємо на сервер
                    int userId = Preferences.Get("UserId", 0);
                    bool success = await _apiService.UploadAvatarAsync(userId, base64String);

                    if (success)
                    {
                        UserProfile.AvatarBase64 = base64String;
                        OnPropertyChanged(nameof(UserProfile));
                        OnPropertyChanged(nameof(HasAvatar));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error picking photo: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public TimeSpan WaterStartTime
        {
            get => _waterService.StartTime;
            set
            {
                if (_waterService.StartTime != value)
                {
                    _waterService.StartTime = value;
                    OnPropertyChanged();
                    _waterService.RestartRemindersInBackground();
                }
            }
        }

        public TimeSpan WaterEndTime
        {
            get => _waterService.EndTime;
            set
            {
                if (_waterService.EndTime != value)
                {
                    _waterService.EndTime = value;
                    OnPropertyChanged();
                    _waterService.RestartRemindersInBackground();
                }
            }
        }

        public ICommand LoadUserProfileCommand { get; }



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
                    UserProfile = await _apiService.GetUserProfileAsync(userId, forceRefresh);
                    _lastUserId = userId;
                    _isDataLoaded = true;
                    OnPropertyChanged(nameof(UserProfile));
                    OnPropertyChanged(nameof(HasAvatar));
                    OnPropertyChanged(nameof(AvatarInitial));
                    Debug.WriteLine($"ProfileViewModel: Loaded - Height={UserProfile?.Height}, Weight={UserProfile?.Weight}");
                }
            }
            finally
            {
                IsLoading = false;
            }

        }

        public async Task CheckStreakAsync()
        {
            int userId = Preferences.Get("UserId", 0);
            if (userId <= 0 || UserProfile == null) return;

            var result = await _apiService.CheckStreakAsync(userId);
            if (result != null)
            {
                UserProfile.CurrentStreak = result.CurrentStreak;
                UserProfile.Balance = result.TotalPoints;
                UserProfile.MonthlyPoints = result.MonthlyPoints;
                
                // Force UI update
                OnPropertyChanged(nameof(UserProfile));
                
                if (result.StreakUpdated)
                {
                    // Show a toast or just log it (optional)
                    Debug.WriteLine($"Streak updated! Earned {result.PointsEarned} points.");
                }
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

    public class AvatarOption : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string ImageSource { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}