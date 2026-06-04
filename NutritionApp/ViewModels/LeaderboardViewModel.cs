using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using NutritionApp.Models;
using NutritionApp.Services;

namespace NutritionApp.ViewModels
{
    public class LeaderboardItem
    {
        public LeaderboardUserDto User { get; set; }
        public int Rank { get; set; }
        public string AvatarChar => !string.IsNullOrEmpty(User?.Username) ? User.Username.Substring(0, 1).ToUpper() : "?";
        public bool HasAvatar => !string.IsNullOrEmpty(User?.AvatarBase64);
    }

    public class LeaderboardViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private bool _isDataLoaded = false;

        public ObservableCollection<LeaderboardItem> LeaderboardUsers { get; set; } = new ObservableCollection<LeaderboardItem>();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadLeaderboardCommand { get; }
        public ICommand RefreshLeaderboardCommand { get; }

        public LeaderboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadLeaderboardCommand = new Command(async () => await LoadLeaderboardAsync(forceRefresh: false));
            RefreshLeaderboardCommand = new Command(async () => await LoadLeaderboardAsync(forceRefresh: true));
        }

        private async Task LoadLeaderboardAsync(bool forceRefresh = false)
        {
            if (_isDataLoaded && !forceRefresh) return;
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                var users = await _apiService.GetLeaderboardAsync();
                LeaderboardUsers.Clear();

                int rank = 1;
                foreach (var u in users)
                {
                    LeaderboardUsers.Add(new LeaderboardItem { User = u, Rank = rank });
                    rank++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadLeaderboardAsync error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                _isDataLoaded = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
