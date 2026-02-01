using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using NutritionApp.Models;
using NutritionApp.Services;
using Microsoft.Maui.Controls;

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
                Debug.WriteLine($"EditProfile: loading profile for userId={userId}");
                if (userId > 0)
                {
                    UserProfile = await _apiService.GetUserProfileAsync(userId);
                    Debug.WriteLine($"EditProfile loaded: username={UserProfile?.Username}, avatarId={UserProfile?.AvatarId}");
                }
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
                Debug.WriteLine($"EditProfile: saving profile for userId={UserProfile.Id}");
                var updatedProfile = await _apiService.UpdateUserProfileAsync(UserProfile.Id, UserProfile);
                if (updatedProfile != null)
                {
                    UserProfile = updatedProfile;
                    Debug.WriteLine("EditProfile: profile updated successfully");
                    await Application.Current.MainPage.DisplayAlert("Успіх", "Профіль оновлено!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    Debug.WriteLine("EditProfile: update returned null");
                    await Application.Current.MainPage.DisplayAlert("Помилка", "Не вдалося оновити профіль.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditProfile: exception while saving - {ex.Message}");
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