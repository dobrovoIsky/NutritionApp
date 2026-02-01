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
            set { _userProfile = value; OnPropertyChanged(); }
        }

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

                // Створюємо payload з правильними назвами полів для сервера (PascalCase)
                var payload = new
                {
                    Height = UserProfile.Height,
                    Weight = UserProfile.Weight,
                    Age = UserProfile.Age,
                    Goal = UserProfile.Goal ?? string.Empty,
                    ActivityLevel = UserProfile.ActivityLevel ?? string.Empty,
                    AvatarId = UserProfile.AvatarId,
                    Gender = UserProfile.Gender ?? "male"
                };

                Debug.WriteLine($"EditProfile: Saving profile for userId: {userId}, payload: Height={payload.Height}, Weight={payload.Weight}");

                var updatedProfile = await _apiService.UpdateUserProfileAsync(userId, payload);
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