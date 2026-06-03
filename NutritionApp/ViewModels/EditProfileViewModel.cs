using System;
using System.ComponentModel;    
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class EditProfileViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        private UserProfile _userProfile;
        public UserProfile UserProfile
        {
            get => _userProfile;
            set 
            { 
                _userProfile = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(SelectedGoal));
                OnPropertyChanged(nameof(SelectedActivityLevel));
                OnPropertyChanged(nameof(SelectedGender));
            }
        }

        public string SelectedGoal
        {
            get => ConvertGoalToUa(_userProfile?.Goal);
            set
            {
                if (_userProfile != null)
                {
                    var enValue = ConvertGoalToEn(value);
                    if (_userProfile.Goal != enValue)
                    {
                        _userProfile.Goal = enValue;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public string SelectedActivityLevel
        {
            get => ConvertActivityToUa(_userProfile?.ActivityLevel);
            set
            {
                if (_userProfile != null)
                {
                    var enValue = ConvertActivityToEn(value);
                    if (_userProfile.ActivityLevel != enValue)
                    {
                        _userProfile.ActivityLevel = enValue;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public string SelectedGender
        {
            get => ConvertGenderToUa(_userProfile?.Gender);
            set
            {
                if (_userProfile != null)
                {
                    var enValue = ConvertGenderToEn(value);
                    if (_userProfile.Gender != enValue)
                    {
                        _userProfile.Gender = enValue;
                        OnPropertyChanged();
                    }
                }
            }
        }

        private string ConvertGoalToUa(string goal) => goal switch
        {
            "lose weight" => "Схуднення",
            "gain muscle" => "Набір маси",
            "maintain weight" => "Підтримка ваги",
            null => "Підтримка ваги",
            _ => "Підтримка ваги"
        };

        private string ConvertGoalToEn(string goal) => goal switch
        {
            "Схуднення" => "lose weight",
            "Набір маси" => "gain muscle",
            "Підтримка ваги" => "maintain weight",
            null => "maintain weight",
            _ => "maintain weight"
        };

        private string ConvertActivityToUa(string activity) => activity switch
        {
            "sedentary" => "Сидячий спосіб життя",
            "lightly active" => "Легка активність",
            "moderately active" => "Помірна активність",
            "very active" => "Висока активність",
            "extra active" => "Дуже висока активність",
            null => "Помірна активність",
            _ => "Помірна активність"
        };

        private string ConvertActivityToEn(string activity) => activity switch
        {
            "Сидячий спосіб життя" => "sedentary",
            "Легка активність" => "lightly active",
            "Помірна активність" => "moderately active",
            "Висока активність" => "very active",
            "Дуже висока активність" => "extra active",
            null => "moderately active",
            _ => "moderately active"
        };

        private string ConvertGenderToUa(string gender) => gender switch
        {
            "male" => "Чоловіча",
            "female" => "Жіноча",
            null => "Чоловіча",
            _ => "Чоловіча"
        };

        private string ConvertGenderToEn(string gender) => gender switch
        {
            "Чоловіча" => "male",
            "Жіноча" => "female",
            null => "male",
            _ => "male"
        };

        public List<string> GoalOptions { get; } = new()
        {
            "Схуднення",
            "Набір маси",
            "Підтримка ваги"
        };

        public List<string> ActivityOptions { get; } = new()
        {
            "Сидячий спосіб життя",
            "Легка активність",
            "Помірна активність",
            "Висока активність",
            "Дуже висока активність"
        };

        public List<string> GenderOptions { get; } = new()
        {
            "Чоловіча",
            "Жіноча"
        };

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public EditProfileViewModel(ApiService apiService)
        {
            _apiService = apiService;
            SaveCommand = new Command(async () => await SaveProfileAsync());
            LoadProfileCommand = new Command(async () => await LoadProfileAsync());
        }

        public ICommand SaveCommand { get; }
        public ICommand LoadProfileCommand { get; }

        private async Task LoadProfileAsync()
        {
            if (IsLoading) return;
            try
            {
                IsLoading = true;
                int userId = Preferences.Get("UserId", 0);
                Debug.WriteLine($"EditProfile: Loading profile for userId: {userId}");
                if (userId > 0)
                {
                    UserProfile = await _apiService.GetUserProfileAsync(userId);
                    Debug.WriteLine($"EditProfile: Loaded - Height={UserProfile?.Height}, Weight={UserProfile?.Weight}, Age={UserProfile?.Age}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditProfile: Load exception: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveProfileAsync()
        {
            if (IsLoading || UserProfile == null) return;
            try
            {
                IsLoading = true;

                int userId = Preferences.Get("UserId", 0);
                if (userId <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося ідентифікувати користувача.", "OK");
                    return;
                }

                // Оновлюємо відсутні значення за замовчуванням перед відправкою
                if (string.IsNullOrEmpty(UserProfile.Goal)) UserProfile.Goal = "maintain weight";
                if (string.IsNullOrEmpty(UserProfile.ActivityLevel)) UserProfile.ActivityLevel = "moderately active";
                if (string.IsNullOrEmpty(UserProfile.Gender)) UserProfile.Gender = "male";

                Debug.WriteLine($"EditProfile: Saving profile for userId: {userId}, payload: Height={UserProfile.Height}, Weight={UserProfile.Weight}, Username={UserProfile.Username}");

                var updatedProfile = await _apiService.UpdateUserProfileAsync(userId, UserProfile);
                if (updatedProfile != null)
                {
                    UserProfile = updatedProfile;
                    await Application.Current.MainPage.DisplayAlert("Успіх", "Профіль оновлено!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося оновити профіль.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditProfile: Save exception: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Помилка", $"Помилка: {ex.Message}", "OK");
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